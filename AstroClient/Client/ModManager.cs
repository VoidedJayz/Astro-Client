using AstroClient.Systems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroClient.Client
{
    internal class ModManager
    {
        public static Version? Version;
        public static bool GodMode = false;
        public static bool InfiniteStamina = false;
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
            bool cancellationFlag = false;
            try
            {
                Console.Clear();
                LogSystem.Log("Starting mod installation process.");
                DiscordManager.UpdatePresence("cog", "Installing Mods");
                if (CheckLethalCompany())
                {
                    LogSystem.Log("Lethal Company is running. Disposing install operation.");
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Lethal Company is running. Please close it before installing mods.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }
                LogSystem.Log("Checked Lethal Company.");

                if (CheckForExistingMods())
                {
                    LogSystem.Log("Existing mods found. Backing up original mods before overwriting.");
                    ConsoleSystem.SetColor(Color.Cyan);
                    ConsoleSystem.AnimatedText("You already have mods, They will be backed up into Astros root dir.");
                    BackupMods();
                    UninstallMods();
                }

                ConsoleSystem.SetColor(Color.Magenta);
                ConsoleSystem.AnimatedText("Downloading Zip... (this may take awhile)");
                LogSystem.Log("Initiating mod pack download.");

                try
                {
                    // Start the elapsed time updater in a separate thread
                    Task.Run(() => UpdateElapsedTime(DateTime.Now));

                    LogSystem.Log("Mod pack download started.");
                    DownloadSystem.ServerDownload(DownloadSystem.AppFiles["modpack"], $"{Program.lethalCompanyPath}\\temp_astro.zip");

                    // Download completed, set the cancellation flag to stop the updater
                    cancellationFlag = true;
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
                MessageBox.Show("Mods Installed!", "Astro Client", MessageBoxButtons.OK, MessageBoxIcon.Information);   
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
            async Task UpdateElapsedTime(DateTime startTime)
            {
                try
                {
                    while (!cancellationFlag)
                    {
                        TimeSpan elapsed = DateTime.Now - startTime;
                        int elapsedSeconds = (int)elapsed.TotalSeconds;
                        int elapsedMilliseconds = elapsed.Milliseconds;

                        Console.Clear(); // Clear the console line
                        Console.WriteLine($"Downloading Zip... Elapsed Time: {elapsedSeconds} seconds {elapsedMilliseconds} ms");

                        await Task.Delay(50); // Update every 50 milliseconds
                    }
                }
                catch (Exception)
                {
                    // Handle exceptions if needed
                }
            }
        }
        public static void UninstallMods()
        {
            try
            {
                Console.Clear();
                LogSystem.Log("Starting mod removal process.");
                DiscordManager.UpdatePresence("cog", "Removing Mods");
                if (CheckLethalCompany())
                {
                    LogSystem.Log("Lethal Company is running. Disposing removal operation.");
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Lethal Company is running. Please close it before removing mods.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

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
                    MessageBox.Show("Mods Removed!", "Astro Client", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Task.Delay(1000).Wait();
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
                Console.Clear();
                LogSystem.Log("Starting mod backup process.");
                DiscordManager.UpdatePresence("cog", "Backing Up Mods");
                if (CheckLethalCompany())
                {
                    LogSystem.Log("Lethal Company is running. Disposing backup operation.");
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Lethal Company is running. Please close it before backing up mods.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }

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
            if (CheckLethalCompany())
            {
                LogSystem.Log($"{pluginName}: Lethal Company is running. Disposing operation.");
                ConsoleSystem.SetColor(Color.DarkRed);
                ConsoleSystem.AnimatedText($"{pluginName}: Lethal Company is running. Please close it before enabling or disabling this plugin.");
                ConsoleSystem.AnimatedText("Press any key to continue...");
                Console.ReadKey();
                return;
            }

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
            if (CheckLethalCompany())
            {
                LogSystem.Log("Lethal Company is running. Disposing operation.");
                ConsoleSystem.SetColor(Color.DarkRed);
                ConsoleSystem.AnimatedText("Lethal Company is running. Please close it before installing shaders.");
                ConsoleSystem.AnimatedText("Press any key to continue...");
                Console.ReadKey();
                return;
            }

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
        public static void VRMode(bool state)
        {
            try
            {
                if (state == true)
                {
                    FileSystem.ReplaceIniValue($"{Program.lethalCompanyPath}\\BepInEx\\config\\io.daxcess.lcvr.cfg", "General", "DisableVR", "false");
                    FileSystem.ReplaceIniValue($"{Program.lethalCompanyPath}\\BepInEx\\config\\FlipMods.HotbarPlus.cfg", "Client-side", "UseDefaultItemSwapInterval", "true");
                    FileSystem.MoveFile($"{Program.pluginsPath}\\CrossHair.dll", $"{Program.bepInExPath}\\core\\CrossHair.dll");
                    FileSystem.MoveFile($"{Program.pluginsPath}\\TooManyEmotes.dll", $"{Program.bepInExPath}\\core\\TooManyEmotes.dll");
                }
                else
                {
                    FileSystem.ReplaceIniValue($"{Program.lethalCompanyPath}\\BepInEx\\config\\io.daxcess.lcvr.cfg", "General", "DisableVR", "true");
                    FileSystem.ReplaceIniValue($"{Program.lethalCompanyPath}\\BepInEx\\config\\FlipMods.HotbarPlus.cfg", "Client-side", "UseDefaultItemSwapInterval", "false");
                    FileSystem.MoveFile($"{Program.bepInExPath}\\core\\CrossHair.dll", $"{Program.pluginsPath}\\CrossHair.dll");
                    FileSystem.MoveFile($"{Program.bepInExPath}\\core\\TooManyEmotes.dll", $"{Program.pluginsPath}\\TooManyEmotes.dll");
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error setting VR Mode. {ex.Message}");
            }
        }
        public static bool CheckLethalCompany()
        {
            Process[] lc = Process.GetProcessesByName("Lethal Company");
            if (lc.Length != 0)
            {
                return true;
            }
            return false;
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
