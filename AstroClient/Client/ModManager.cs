using AstroClient.Systems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Client
{
    internal class ModManager
    {
        public static Version? Version;
        public static void Start()
        {
            CheckMods();
        }
        public static void CheckMods()
        {
            if (Program.lethalCompanyPath == null)
            {
                LogSystem.Log("Lethal Company path is null, cannot check mods.");
                return;
            }

            if (Program.bepInExPath == null)
            {
                LogSystem.Log("BepInEx path is null, cannot check mods.");
                return;
            }

            if (Program.pluginsPath == null)
            {
                LogSystem.Log("Plugins path is null, cannot check mods.");
                return;
            }

            if (!FileSystem.DirectoryExists(Program.lethalCompanyPath))
            {
                LogSystem.Log("Lethal Company path does not exist, cannot check mods.");
                return;
            }

            if (!FileSystem.DirectoryExists(Program.bepInExPath))
            {
                LogSystem.Log("BepInEx path does not exist, cannot check mods.");
                return;
            }

            if (!FileSystem.DirectoryExists(Program.pluginsPath))
            {
                LogSystem.Log("Plugins path does not exist, cannot check mods.");
                return;
            }

            LogSystem.Log("Checking for mods...");

            var mods = FileSystem.GetFiles(Program.pluginsPath, "*.dll", SearchOption.AllDirectories);
            int modCount = 0;
            if (mods.Count == 0)
            {
                LogSystem.Log("No mods found.");
            }
            else
            {
                foreach (string mod in mods)
                {
                    string modName = mod.Split('\\').Last();
                    LogSystem.Log($"Found mod: {modName}");
                    modCount++;
                }
                LogSystem.Log($"Found {modCount} mods.");
            }

            LogSystem.Log("Getting Local Modpack Version.");
            if (FileSystem.FileExists($"{Program.bepInExPath}\\Version"))
            {
                Version = new Version(FileSystem.ReadAllText($"{Program.bepInExPath}\\Version"));
                LogSystem.Log($"Local Modpack Version: {Version}");
            }
            else
            {
                Version = new Version("0.0.0");
                LogSystem.Log($"Local Modpack Version: {Version}");
            }
        }
        public static void InstallMods()
        {
            try
            {
                LogSystem.Log("Starting mod installation process.");
                DiscordManager.UpdatePresence("cog", "Installing Mods");
                CheckLethalCompany();
                LogSystem.Log("Checked Lethal Company.");

                if (CheckForExistingMods())
                {
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Existing Mods Found. Please remove them before installing our pack.");
                    ConsoleSystem.AnimatedText("You can use option 2 on the main menu to remove the mods.");
                    ConsoleSystem.AnimatedText("If you care about your mods, please back them up using option 6 before removing them.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                    LogSystem.Log("Existing mods found. Aborting installation.");
                    return;
                }

                ConsoleSystem.SetColor(Color.Magenta);
                ConsoleSystem.AnimatedText("Downloading Zip...");
                LogSystem.Log("Initiating mod pack download.");

                try
                {
                    LogSystem.Log("Mod pack download started.");
                    DownloadSystem.ServerDownload(DownloadSystem.AppFiles["modpack"], $"{Program.lethalCompanyPath}\\temp_astro.zip");
                }
                catch (Exception ex)
                {
                    ConsoleSystem.AnimatedText($"Error Downloading Mods: {ex}");
                    LogSystem.ReportError($"Error downloading mods: {ex}");
                    Task.Delay(2500).Wait();
                    return;
                }

                ConsoleSystem.AnimatedText("Zip Download Complete!");
                ConsoleSystem.AnimatedText("Extracting Zip...");
                ConsoleSystem.SetColor(Color.Magenta);
                LogSystem.Log("Mod pack zip download complete. Extracting zip.");

                using (var archive = ZipFile.OpenRead($"{Program.lethalCompanyPath}\\temp_astro.zip"))
                {
                    using (var progress = new ProgressBar())
                    {
                        int totalEntries = archive.Entries.Count;
                        int entryIndex = 0;

                        foreach (var entry in archive.Entries)
                        {
                            // Skip directories
                            if (string.IsNullOrEmpty(entry.Name))
                                continue;

                            string destinationPath = Path.Combine(Program.lethalCompanyPath, entry.FullName);
                            string destinationDir = Path.GetDirectoryName(destinationPath);

                            if (!Directory.Exists(destinationDir))
                                Directory.CreateDirectory(destinationDir);

                            entry.ExtractToFile(destinationPath, overwrite: true);

                            entryIndex++;
                            progress.Report((double)entryIndex / totalEntries);
                            Console.Title = $"ASTRO BOYZ! | Installing Mods... | {entryIndex}/{totalEntries}";
                        }
                    }
                }

                FileSystem.WriteAllText($"{Program.bepInExPath}\\Version", ServerManager.latestModpackVersion.ToString());
                ConsoleSystem.SetColor(Color.Green);
                ConsoleSystem.AnimatedText("Mods installation complete.");
                FileSystem.DeleteFile($"{Program.lethalCompanyPath}\\temp_astro.zip");
                LogSystem.Log("Mod pack installation complete.");
                Task.Delay(2500).Wait();
            }
            catch (Exception ex)
            {
                ConsoleSystem.SetColor(Color.DarkRed);
                ConsoleSystem.AnimatedText($"Error Installing Mods. Check logs for details.");
                LogSystem.ReportError($"Error in InstallMods: {ex}");
            }
            finally
            {
                Console.Clear();
                LogSystem.Log("Mod installation process finished.");
            }
        }

        public static void UninstallMods()
        {
            try
            {
                LogSystem.Log("Starting mod removal process.");
                DiscordManager.UpdatePresence("cog", "Removing Mods");
                CheckLethalCompany();

                // Check For Currently Installed Modz
                if (CheckForExistingMods() == true)
                {
                    ConsoleSystem.SetColor(Color.Cyan);
                    LogSystem.Log("Existing mods found. Beginning removal process.");

                    try
                    {
                        FileSystem.DeleteDirectory(Program.bepInExPath);
                        LogSystem.Log($"Deleted directory: {Program.bepInExPath}");
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ReportError($"Error deleting directory (BepInExPath): {ex}");
                    }
                    try
                    {
                        FileSystem.DeleteFile($"{Program.lethalCompanyPath}\\doorstop_config.ini");
                        LogSystem.Log($"Deleted file: {Program.lethalCompanyPath}\\doorstop_config.ini");
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ReportError($"Error deleting file (doorstop_config.ini): {ex}");
                    }
                    try
                    {
                        FileSystem.DeleteFile($"{Program.lethalCompanyPath}\\winhttp.dll");
                        LogSystem.Log($"Deleted file: {Program.lethalCompanyPath}\\winhttp.dll");
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ReportError($"Error deleting file (winhttp.dll): {ex}");
                    }

                    ConsoleSystem.SetColor(Color.Green);
                    ConsoleSystem.AnimatedText("All mod files removed!");
                    LogSystem.Log("All mod files have been successfully removed.");
                    Task.Delay(2500).Wait();
                }
                else
                {
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("No mods installed.");
                    LogSystem.Log("No mods installed. Nothing to remove.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in RemoveModz: {ex}");
            }
            finally
            {
                Console.Clear();
                LogSystem.Log("Mod removal process finished.");
            }
        }
        public static void BackupMods()
        {
            try
            {
                LogSystem.Log("Starting mod backup process.");
                DiscordManager.UpdatePresence("cog", "Backing Up Mods");
                CheckLethalCompany();

                // Check For Currently Installed Modz
                if (CheckForExistingMods() == true)
                {
                    var dir = FileSystem.CreateUniqueFolderName("Mod-BackUp-", $"{Directory.GetCurrentDirectory()}\\Backups\\");
                    ConsoleSystem.SetColor(Color.Cyan);
                    LogSystem.Log("Existing mods found. Beginning backup process.");

                    try
                    {
                        FileSystem.CopyDirectory(Program.bepInExPath, $"{dir}\\BepInEx");
                        LogSystem.Log($"Copied directory: {Program.bepInExPath} to {dir}\\BepInEx");
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ReportError($"Error copying directory (BepInExPath): {ex}");
                    }
                    try
                    {
                        FileSystem.CopyFile($"{Program.lethalCompanyPath}\\doorstop_config.ini", $"{dir}\\doorstop_config.ini");
                        LogSystem.Log($"Copied file: {Program.lethalCompanyPath}\\doorstop_config.ini to {Program.lethalCompanyPath}\\doorstop_config.ini");
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ReportError($"Error copying file (doorstop_config.ini): {ex}");
                    }
                    try
                    {
                        FileSystem.CopyFile($"{Program.lethalCompanyPath}\\winhttp.dll", $"{dir}\\winhttp.dll");
                        LogSystem.Log($"Copied file: {Program.lethalCompanyPath}\\winhttp.dll to {dir}\\winhttp.dll");
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ReportError($"Error copying file (winhttp.dll): {ex}");
                    }

                    ConsoleSystem.SetColor(Color.Green);
                    LogSystem.Log("All mod files have been successfully backed up.");
                    Task.Delay(2500).Wait();
                }
                else
                {
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("No mods installed.");
                    LogSystem.Log("No mods installed. Nothing to backup.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in BackupMods: {ex}");
            }
            finally
            {
                LogSystem.Log("Mod backup process finished.");
                ConsoleSystem.AnimatedText("Mod Backup Finished.");
                ConsoleSystem.AnimatedText($"You can find your backups here: {Directory.GetCurrentDirectory()}\\Backups");
                ConsoleSystem.AnimatedText("Press any key to continue...");
                Console.ReadKey();
            }
        }
        public static void ProcessPlugin(string pluginName, bool state, string enabledPath, string disabledPath, string alreadyStateMessage)
        {
            LogSystem.Log($"{pluginName}: Start processing.");
            CheckLethalCompany();

            try
            {
                if (state)
                {
                    // Enable
                    if (!FileSystem.FileExists(disabledPath) || FileSystem.FileExists(enabledPath))
                    {
                        ConsoleSystem.AnimatedText($"{pluginName} is already enabled.");
                        LogSystem.Log($"{pluginName} is already enabled.");
                    }
                    else
                    {
                        FileSystem.MoveFile(disabledPath, enabledPath);
                        LogSystem.Log($"{pluginName} enabled.");
                    }
                }
                else
                {
                    // Disable
                    if (!FileSystem.FileExists(enabledPath) || FileSystem.FileExists(disabledPath))
                    {
                        ConsoleSystem.AnimatedText($"{pluginName} is already disabled.");
                        LogSystem.Log($"{pluginName} is already disabled.");
                    }
                    else
                    {
                        FileSystem.MoveFile(enabledPath, disabledPath);
                        LogSystem.Log($"{pluginName} disabled.");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleSystem.SetColor(Color.DarkRed);
                ConsoleSystem.AnimatedText($"An error occured while processing. Check log file for details.");
                LogSystem.ReportError($"{pluginName} error: {ex}");
                Task.Delay(2500).Wait();
            }

            LogSystem.Log($"{pluginName}: End processing.");
        }
        public static void AstroMenu(bool state)
        {
            ProcessPlugin("AstroMenu", state, $"{Program.pluginsPath}\\AstroMenu.dll", $"{Program.bepInExPath}\\core\\AstroMenu.dll", "AstroMenu");
        }
        public static void BrutalCompany(bool state)
        {
            ProcessPlugin("BrutalCompany", state, $"{Program.pluginsPath}\\BrutalCompanyPlus.dll", $"{Program.bepInExPath}\\core\\BrutalCompanyPlus.dll", "BrutalCompany");
        }
        public static void RichPresence(bool state)
        {
            ProcessPlugin("RichPresence", state, $"{Program.pluginsPath}\\AstroRPC.dll", $"{Program.bepInExPath}\\core\\AstroRPC.dll", "RichPresence");
        }
        public static void RetroShading(bool state)
        {
            LogSystem.Log("Checking for existing shaders...");
            CheckLethalCompany();

            if (state)
            {
                if (CheckForExistingShaders())
                {
                    LogSystem.Log("Existing shaders found. Please remove them before installing new shaders.");
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Existing Shaders Found. Please remove them before installing new shaders.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

                LogSystem.Log("Downloading shaders...");
                ConsoleSystem.SetColor(Color.Magenta);
                ConsoleSystem.AnimatedText("Downloading...");

                try
                {
                    DownloadSystem.ServerDownload(DownloadSystem.AppFiles["reshade"], $"{Program.lethalCompanyPath}\\setup.exe");
                    DownloadSystem.ServerDownload(DownloadSystem.AppFiles["shader"], $"{Program.lethalCompanyPath}\\RetroShader.ini");
                    DownloadSystem.ServerDownload(DownloadSystem.AppFiles["instructions"], $"{Program.lethalCompanyPath}\\instructions.txt");
                }
                catch (Exception ex)
                {
                    LogSystem.ReportError($"Error downloading shaders: {ex}");
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText($"Error Downloading Shaders. Check logs for details.");
                    Task.Delay(2500).Wait();
                    return;
                }

                ConsoleSystem.AnimatedText("Opening Setup...");
                ConsoleSystem.SetColor(Color.Magenta);
                Process p1 = null;
                Process p2 = null;

                try
                {
                    p1 = Process.Start($"{Program.lethalCompanyPath}\\setup.exe");
                    p2 = Process.Start($"{Program.lethalCompanyPath}\\instructions.txt");
                    ConsoleSystem.SetForegroundWindow(p1.MainWindowHandle);
                    ConsoleSystem.SetForegroundWindow(p2.MainWindowHandle);
                }
                catch (Exception ex)
                {
                    LogSystem.ReportError($"Error starting processes: {ex}");
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText($"Error Starting Processes. Check log file for details.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                }

                ConsoleSystem.SetColor(Color.DarkRed);
                ConsoleSystem.AnimatedText("Please follow the instructions in the window(s) that were opened!!!!");

                while (p1.HasExited == false)
                {
                    Console.Title = $"ASTRO BOYZ! | Waiting for Setup to Finish or Exit... | {p1.MainWindowTitle}";
                    Task.Delay(25).Wait();
                }

                if (FileSystem.FileExists($"{Program.lethalCompanyPath}\\ReShade.ini"))
                {
                    FileSystem.ReplaceIniValue($"{Program.lethalCompanyPath}\\ReShade.ini", "INPUT", "KeyOverlay", "73,1,0,0");
                }
            }
            else
            {
                if (CheckForExistingShaders() == false)
                {
                    LogSystem.Log("No shaders found.");
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("No shaders found.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }
                else
                {
                    LogSystem.Log("Removing shaders...");
                    FileSystem.DeleteDirectory(Program.lethalCompanyPath + "\\reshade-shaders");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\dxgi.dll");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\ReShade.ini");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\ReShade.log");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\ReShadePreset.ini");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\setup.exe");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\RetroShader.ini");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\instructions.txt");
                    LogSystem.Log("Shaders Removed!");
                    ConsoleSystem.SetColor(Color.Green);
                    ConsoleSystem.AnimatedText("Shaders Removed!");
                    Task.Delay(2500).Wait();
                    return;
                }
            }
        }
        public static void CheckLethalCompany()
        {
            Process[] lc = Process.GetProcessesByName("Lethal Company");
            if (lc.Length != 0)
            {
                ConsoleSystem.SetColor(Color.DarkRed);
                Console.Clear();
                ConsoleSystem.AnimatedText("Lethal Company is currently running. Please close the game before continuing.\n");
                ConsoleSystem.AnimatedText("Press any key to continue...");
                Console.ReadKey();
                CheckLethalCompany();
            }
        }
        public static bool CheckForExistingShaders()
        {
            var DetectedFiles = new List<string>()
            {
                $"{Program.lethalCompanyPath}\\dxgi.dll",
                $"{Program.lethalCompanyPath}\\ReShade.ini",
                $"{Program.lethalCompanyPath}\\ReShade.log",
                $"{Program.lethalCompanyPath}\\ReShadePreset.ini",
                $"{Program.lethalCompanyPath}\\RetroShader.ini",
                $"{Program.lethalCompanyPath}\\setup.exe",
            };
            if (FileSystem.DirectoryExists(Program.lethalCompanyPath + "\\reshade-shaders"))
            {
                return true;
            }
            if (DetectedFiles.Any(FileSystem.FileExists))
            {
                return true;
            }
            return false;
        }
        public static bool CheckForExistingMods()
        {
            if (FileSystem.DirectoryExists(Program.bepInExPath) || FileSystem.DirectoryExists($"{Program.lethalCompanyPath}\\winhttp.dll") || FileSystem.DirectoryExists($"{Program.lethalCompanyPath}\\doorstop_config.ini"))
            {
                return true;
            }
            return false;
        }
    }
}
