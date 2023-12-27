using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroClient.Systems
{
    internal class SteamSystem
    {
        public static void LaunchSteamGame(int game)
        {
            try
            {
                LogSystem.Log($"Attempting to launch Steam game with ID: {game}");
                Process.Start("steam://rungameid/" + game);
                LogSystem.Log("Steam game launched successfully.");
            }
            catch (Exception ex)
            {
                ConsoleSystem.SetColor(Color.DarkRed);
                ConsoleSystem.AnimatedText($"Error Starting Steam game: {ex.Message}");
                LogSystem.ReportError($"Error starting Steam game: {ex.Message}");
            }
        }
        public static void CloseSteamGame(int appId)
        {
            string steamUrl = $"steam://nav/games/details/{appId}";

            try
            {
                LogSystem.Log($"Attempting to close Steam game with AppID: {appId}");
                Process.Start(steamUrl);

                Process[] processes = Process.GetProcessesByName("Lethal Company");
                foreach (Process process in processes)
                {
                    process.CloseMainWindow();
                    process.WaitForExit();

                    LogSystem.Log($"Closed process: {process.ProcessName}");
                }
                LogSystem.Log("All processes associated with the game have been closed.");
            }
            catch (Exception ex)
            {
                ConsoleSystem.SetColor(Color.DarkRed);
                ConsoleSystem.AnimatedText($"Error closing Steam game: {ex.Message}");
                LogSystem.ReportError($"Error closing Steam game: {ex.Message}");
            }
        }

        public static async Task Start()
        {
            try
            {
                LogSystem.Log("Starting to check Steam install data.");

                Colorful.Console.Clear();

                var allLibraryFolders = GetAllSteamLibraryFolders();
                LogSystem.Log($"Found {allLibraryFolders.Count} Steam library folders.");

                foreach (var steamAppsFolder in allLibraryFolders)
                {
                    string[] appManifestFiles = Directory.GetFiles(steamAppsFolder, "appmanifest_*.acf");
                    LogSystem.Log($"Found {appManifestFiles.Length} app manifest files in {steamAppsFolder}.");

                    foreach (var file in appManifestFiles)
                    {
                        string[] lines = File.ReadAllLines(file);
                        string appName = "";
                        string installDir = "";
                        string fullPath = "";

                        foreach (var line in lines)
                        {
                            if (line.Contains("\"name\""))
                                appName = ExtractValue(line);
                            if (line.Contains("\"installdir\""))
                                installDir = ExtractValue(line);
                        }

                        if (!string.IsNullOrEmpty(installDir))
                        {
                            fullPath = Path.Combine(steamAppsFolder, "common", installDir);
                            LogSystem.Log($"Found game: {appName} at {fullPath}");
                        }

                        if (appName == "Lethal Company")
                        {
                            Program.lethalCompanyPath = fullPath;
                            Program.bepInExPath = Path.Combine(Program.lethalCompanyPath, "BepInEx");
                            Program.pluginsPath = Path.Combine(Program.lethalCompanyPath, "BepInEx", "plugins");
                            LogSystem.Log("Paths set.");
                            return;
                        }
                    }
                }

                await Task.Delay(100);
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in CheckSteamInstallData: {ex.Message}");
            }
            if (!FileSystem.DirectoryExists(Program.lethalCompanyPath))
            {
                try
                {
                    LogSystem.Log("Starting to get folder path.");

                    ConfigSystem.ResetCurrentConfigPath();
                    string folderPath = PromptForFolderPath();

                    if (folderPath.Contains('"'))
                    {
                        folderPath = AdjustFolderPath(folderPath);
                    }

                    SaveAndSetPath(folderPath);
                }
                catch (Exception ex)
                {
                    LogSystem.ReportError($"Error in GetFolderPath: {ex.Message}");
                }
            }
        }
        public static string GetSteamPath()
        {
            string steamPath;
            string steamRegKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam";
            string steamRegKey64 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam";
            string steamRegValue = "InstallPath";

            if (Environment.Is64BitOperatingSystem)
            {
                var regFound = Registry.GetValue(steamRegKey64, steamRegValue, null);
                steamPath = regFound.ToString();
            }
            else
            {
                var regFound = Registry.GetValue(steamRegKey, steamRegValue, null);
                steamPath = regFound.ToString();
            }
            return steamPath;
        }
        private static List<string> GetAllSteamLibraryFolders()
        {
            List<string> libraryFolders = new List<string>();
            string mainSteamPath = GetSteamPath();

            if (!string.IsNullOrEmpty(mainSteamPath))
            {
                libraryFolders.Add(Path.Combine(mainSteamPath, "steamapps"));
            }

            // Scan each drive for Steam library folders
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady)
                {
                    string potentialSteamPath = Path.Combine(drive.Name, "Steam", "steamapps");
                    if (FileSystem.DirectoryExists(potentialSteamPath))
                    {
                        libraryFolders.Add(potentialSteamPath);
                    }
                }
            }

            // Parse the libraryfolders.vdf file for additional library folders
            string libraryFoldersFile = Path.Combine(mainSteamPath, "steamapps", "libraryfolders.vdf");
            if (FileSystem.FileExists(libraryFoldersFile))
            {
                string[] lines = File.ReadAllLines(libraryFoldersFile);
                foreach (string line in lines)
                {
                    if (line.Contains("\t\t\"path\"\t\t"))
                    {
                        string path = ExtractValue(line);
                        libraryFolders.Add(Path.Combine(path, "steamapps"));
                    }
                }
            }

            return libraryFolders.Distinct().ToList(); // Remove duplicates, if any
        }
        private static string ExtractValue(string line)
        {
            var match = Regex.Match(line, "\"[^\"]+\"\\s+\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : "";
        }
        private static string PromptForFolderPath()
        {
            Console.Clear();
            ConsoleSystem.SetColor(Color.White);
            ConsoleSystem.AnimatedText("Uh oh, looks like we were unable to find the folder path for Lethal Company.");
            ConsoleSystem.AnimatedText("Example: C:\\Program Files (x86)\\Steam\\steamapps\\common\\Lethal Company");
            ConsoleSystem.AnimatedText("Please type a folder path or you can just press 'Enter' to select a folder.");
            string folderPath = Console.ReadLine();
            Console.WriteLine();
            LogSystem.Log($"User entered folder path: {folderPath}");
            return folderPath;
        }

        private static string AdjustFolderPath(string folderPath)
        {
            string adjustedPath = folderPath.Split('"')[1];
            LogSystem.Log($"Folder path adjusted from user input: {adjustedPath}");
            return adjustedPath;
        }

        private static void SaveAndSetPath(string folderPath)
        {
            if (!string.IsNullOrWhiteSpace(folderPath))
            {
                ConsoleSystem.AnimatedText(folderPath);
                ConsoleSystem.AnimatedText("Path Saved!");

                Program.lethalCompanyPath = folderPath;
                ConfigSystem.loadedConfig.customPath = folderPath;
                ConfigSystem.loadedConfig.Save();
                Program.lethalCompanyPath = folderPath;
                Program.bepInExPath = Path.Combine(Program.lethalCompanyPath, "BepInEx");
                Program.pluginsPath = Path.Combine(Program.lethalCompanyPath, "BepInEx", "plugins");
                LogSystem.Log($"Path saved and set: {folderPath}");
            }
        }

    }
}
