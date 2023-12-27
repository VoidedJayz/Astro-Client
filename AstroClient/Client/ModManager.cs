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
        public static async Task Start()
        {
            await CheckMods();
        }
        public static async Task CheckMods()
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

            var mods = FileSystem.GetFiles(Program.pluginsPath, ".", SearchOption.TopDirectoryOnly);
            int modCount = 0;
            if (mods.Count == 0)
            {
                LogSystem.Log("No mods found.");
                return;
            }
            else
            {
                foreach (string mod in mods)
                {
                    string modName = mod.Split('\\').Last();
                    LogSystem.Log($"Found mod: {modName}");
                    modCount++;
                }
            }

            LogSystem.Log($"Found {modCount} mods.");
        }
        public static void InstallMods()
        {
            try
            {
                LogSystem.Log("Starting mod installation process.");
                DiscordManager.UpdatePresence("cog", "Installing Mods");
                CheckLethalCompany();
                LogSystem.Log("Checked Lethal Company.");

                if (CheckForExistingMods() == true)
                {
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Existing Mods Found. Please remove them before installing our pack.");
                    ConsoleSystem.AnimatedText("You can use option 2 on the main menu to remove the mods.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                    LogSystem.Log("Existing mods found. Aborting installation.");
                    return;
                }

                using (var client = new WebClient())
                {
                    // More spaghetti code yayyyyy
                    ConsoleSystem.SetColor(Color.Magenta);
                    ConsoleSystem.AnimatedText("Downloading Zip...");
                    LogSystem.Log("Initiating mod pack download.");

                    try
                    {
                        client.DownloadProgressChanged += (s, e) =>
                        {
                            Console.Title = $"ASTRO BOYZ! | Downloading... | {e.ProgressPercentage}% ({e.BytesReceived}/{e.TotalBytesToReceive})";
                        };
                        client.DownloadFileAsync(new Uri(UpdateManager.AppFiles["modpack"]), $"{Program.lethalCompanyPath}\\temp_astro.zip");
                        LogSystem.Log("Mod pack download started.");
                    }
                    catch (Exception ex)
                    {
                        ConsoleSystem.AnimatedText($"Error Downloading Mods: {ex.Message}");
                        LogSystem.ReportError($"Error downloading mods: {ex.Message}");
                        Thread.Sleep(2500);
                        return;
                    }

                    while (client.IsBusy)
                    {
                        Thread.Sleep(500);
                    }
                    ConsoleSystem.AnimatedText("Zip Download Complete!");
                    ConsoleSystem.AnimatedText("Opening Zip...");
                    ConsoleSystem.SetColor(Color.Magenta);
                    LogSystem.Log("Mod pack zip download complete. Opening zip.");

                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead($"{Program.lethalCompanyPath}\\temp_astro.zip"))
                        {
                            using (var progress = new ProgressBar())
                            {
                                for (int i = 0; i <= archive.Entries.Count; i++)
                                {
                                    progress.Report((double)i / archive.Entries.Count);
                                    Console.Title = $"ASTRO BOYZ! | Installing Modz... | {i}/{archive.Entries.Count}";
                                    Thread.Sleep(1);
                                }
                            }
                        }
                        Console.Write($"                                                                    ");
                        Console.SetCursorPosition(0, Console.CursorTop);
                        ConsoleSystem.SetColor(Color.Cyan);
                        ConsoleSystem.AnimatedText("Extracting Files...");
                        LogSystem.Log("Extracting files from mod pack zip.");
                        ZipFile.ExtractToDirectory($"{Program.lethalCompanyPath}\\temp_astro.zip", $"{Program.lethalCompanyPath}");

                        ConsoleSystem.SetColor(Color.Green);
                        ConsoleSystem.AnimatedText("Finished!");
                        FileSystem.DeleteFile($"{Program.lethalCompanyPath}\\temp_astro.zip");
                        LogSystem.Log("Mod pack installation complete.");
                        Thread.Sleep(2500);
                    }
                    catch (Exception ex)
                    {
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText($"Error Extracting Files: {ex.Message}");
                        LogSystem.ReportError($"Error extracting files: {ex.Message}");
                        ConsoleSystem.AnimatedText("Perhaps the file was corrupted?");
                        ConsoleSystem.AnimatedText("Press any key to continue...");
                        Console.ReadKey();
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in InstallModz: {ex.Message}");
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
                        LogSystem.ReportError($"Error deleting directory (BepInExPath): {ex.Message}");
                    }
                    try
                    {
                        FileSystem.DeleteFile($"{Program.lethalCompanyPath}\\doorstop_config.ini");
                        LogSystem.Log($"Deleted file: {Program.lethalCompanyPath}\\doorstop_config.ini");
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ReportError($"Error deleting file (doorstop_config.ini): {ex.Message}");
                    }
                    try
                    {
                        FileSystem.DeleteFile($"{Program.lethalCompanyPath}\\winhttp.dll");
                        LogSystem.Log($"Deleted file: {Program.lethalCompanyPath}\\winhttp.dll");
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ReportError($"Error deleting file (winhttp.dll): {ex.Message}");
                    }

                    ConsoleSystem.SetColor(Color.Green);
                    ConsoleSystem.AnimatedText("All mod files removed!");
                    LogSystem.Log("All mod files have been successfully removed.");
                    Thread.Sleep(2500);
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
                LogSystem.ReportError($"Error in RemoveModz: {ex.Message}");
            }
            finally
            {
                Console.Clear();
                LogSystem.Log("Mod removal process finished.");
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
                ConsoleSystem.AnimatedText($"Error: {ex.Message}");
                LogSystem.ReportError($"{pluginName} error: {ex.Message}");
                Thread.Sleep(2000);
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
            // Too lazy to make logs for this lol
            CheckLethalCompany();
            if (state == true)
            {
                if (CheckForExistingShaders() == true)
                {
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Existing Shaders Found. Please remove them before installing new shaders.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }
                using (var client = new WebClient())
                {
                    ConsoleSystem.SetColor(Color.Magenta);
                    ConsoleSystem.AnimatedText("Downloading...");
                    try
                    {
                        client.DownloadProgressChanged += (s, e) =>
                        {
                            Console.Title = $"ASTRO BOYZ! | Downloading... | {e.ProgressPercentage}% ({e.BytesReceived}/{e.TotalBytesToReceive})";
                        };
                        client.DownloadFileAsync(new Uri(UpdateManager.AppFiles["reshade"]), $"{Program.lethalCompanyPath}\\setup.exe");
                        while (client.IsBusy)
                        {
                            Thread.Sleep(500);
                        }
                        client.DownloadFileAsync(new Uri(UpdateManager.AppFiles["shader"]), $"{Program.lethalCompanyPath}\\RetroShader.ini");
                        while (client.IsBusy)
                        {
                            Thread.Sleep(500);
                        }
                        client.DownloadFileAsync(new Uri(UpdateManager.AppFiles["instructions"]), $"{Program.lethalCompanyPath}\\intructions.txt");
                    }
                    catch (Exception ex)
                    {
                        ConsoleSystem.AnimatedText($"Error Downloading Shaders: {ex.Message}");
                        LogSystem.ReportError($"Error downloading shaders: {ex.Message}");
                        Thread.Sleep(2500);
                        return;
                    }
                    ConsoleSystem.AnimatedText("Opening Setup...");
                    ConsoleSystem.SetColor(Color.Magenta);
                    Process p1 = null;
                    Process p2 = null;
                    try
                    {
                        p1 = Process.Start($"{Program.lethalCompanyPath}\\setup.exe");
                        p2 = Process.Start($"{Program.lethalCompanyPath}\\intructions.txt");
                        ConsoleSystem.SetForegroundWindow(p1.MainWindowHandle);
                        ConsoleSystem.SetForegroundWindow(p2.MainWindowHandle);
                    }
                    catch (Exception ex)
                    {
                        LogSystem.ReportError($"Error starting processes: {ex.Message}");
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText($"Error Starting Processes: {ex.Message}");
                        ConsoleSystem.AnimatedText("Perhaps the file was corrupted?");
                        ConsoleSystem.AnimatedText("Press any key to continue...");
                        Console.ReadKey();
                    }
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Please follow the instructions in the window(s) that were opened!!!!");
                    while (p1.HasExited == false)
                    {
                        Console.Title = $"ASTRO BOYZ! | Waiting for Setup to Finish or Exit... | {p1.MainWindowTitle}";
                        Thread.Sleep(500);
                    }
                    if (FileSystem.FileExists($"{Program.lethalCompanyPath}\\ReShade.ini"))
                    {
                        FileSystem.ReplaceIniValue($"{Program.lethalCompanyPath}\\ReShade.ini", "INPUT", "KeyOverlay", "73,1,0,0");
                    }
                    Thread.Sleep(2000);
                }
            }
            else
            {
                if (CheckForExistingShaders() == false)
                {
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("No shaders found.");
                    ConsoleSystem.AnimatedText("Press any key to continue...");
                    Console.ReadKey();
                    return;
                }
                else
                {
                    FileSystem.DeleteDirectory(Program.lethalCompanyPath + "\\reshade-shaders");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\dxgi.dll");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\ReShade.ini");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\ReShade.log");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\ReShadePreset.ini");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\setup.exe");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\RetroShader.ini");
                    FileSystem.DeleteFile(Program.lethalCompanyPath + "\\intructions.txt");
                    ConsoleSystem.SetColor(Color.Green);
                    ConsoleSystem.AnimatedText("Shaders Removed!");
                    Thread.Sleep(1500);
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
