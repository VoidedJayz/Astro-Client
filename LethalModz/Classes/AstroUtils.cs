using DiscordRPC;
using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using Newtonsoft.Json;
using Objects;
using LethalModz.Classes;

namespace Astro.Classes
{
    public class AstroUtils
    {
        public static AstroConfig currentConfig;
        public static WebClient server = new WebClient();
        public static Version currentVersion = new Version("2.5.7");
        public static string lethalCompanyPath = null;
        public static string currentSteamId = null;
        public static string currentSteamName = null;
        public static string bepInExPath = $"{lethalCompanyPath}\\BepInEx";
        public static string pluginsPath = $"{bepInExPath}\\plugins";
        public static string brutalState;
        public static string astroState;
        public static string richState;
        public static string retroState;
        // Runtime Variables, Not Apart of Config
        public static Dictionary<string, string> files = new Dictionary<string, string>()
        {
            { "astro", "https://cdn.astroswrld.club/Client/LethalModz.exe" },
            { "modpack", "https://cdn.astroswrld.club/Client/modpack.zip" },
            { "shader", "https://cdn.astroswrld.club/Client/RetroShader.ini" },
            { "instructions", "https://cdn.astroswrld.club/Client/shaderinstructions.txt" },
            { "reshade", "https://reshade.me/downloads/ReShade_Setup_5.9.2.exe" },
        };
        public static Dictionary<string, string> dependencies = new Dictionary<string, string>()
        {
            { "Colorful.Console.dll", "https://cdn.astroswrld.club/Client/Dependencies/Colorful.Console.dll" },
            { "NAudio.dll", "https://cdn.astroswrld.club/Client/Dependencies/NAudio.dll" },
            { "NAudio.Core.dll", "https://cdn.astroswrld.club/Client/Dependencies/NAudio.Core.dll" },
            { "NAudio.Wasapi.dll", "https://cdn.astroswrld.club/Client/Dependencies/NAudio.Wasapi.dll" },
            { "NAudio.WinMM.dll", "https://cdn.astroswrld.club/Client/Dependencies/NAudio.WinMM.dll" },
            { "DiscordRPC.dll", "https://cdn.astroswrld.club/Client/Dependencies/DiscordRPC.dll" },
            { "Newtonsoft.Json.dll", "https://cdn.astroswrld.club/Client/Dependencies/Newtonsoft.Json.dll" },
            { "MenuMusic\\FLAUNT.mp3", "https://cdn.astroswrld.club/Client/Dependencies/FLAUNT.mp3" },
            { "MenuMusic\\Drag_Me_Down.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Drag_Me_Down.mp3" },
            { "MenuMusic\\RAPTURE.mp3", "https://cdn.astroswrld.club/Client/Dependencies/RAPTURE.mp3" },
            { "MenuMusic\\I_Am_All_Of_Me.mp3", "https://cdn.astroswrld.club/Client/Dependencies/I_Am_All_Of_Me.mp3" },
            { "MenuMusic\\Painkiller.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Painkiller.mp3" },
            { "MenuMusic\\messages_from_the_stars.mp3", "https://cdn.astroswrld.club/Client/Dependencies/messages_from_the_stars.mp3" },
            { "MenuMusic\\Candle_Queen.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Candle_Queen.mp3" },
            { "MenuMusic\\Just_An_Attraction.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Just_An_Attraction.mp3" }
        };

        // Console Functions
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        public static void ConsoleAnimation(string input)
        {
            // Slowly Type Out Text
            var chars = input.ToCharArray();
            foreach (var letter in chars)
            {
                Colorful.Console.Write(letter);
                Thread.Sleep(10);
            }
            Colorful.Console.WriteLine();
        }
        public static void SetColor(Color color)
        {
            // Why tf did I make this? Fucking useless, just to look cleaner ig
            Colorful.Console.ForegroundColor = color;
        }
        public static void CenterText(string text)
        {
            // Center the input text on the console
            Console.WriteLine(text.PadLeft((Console.WindowWidth / 2) + (text.Length / 2)).PadRight(Console.WindowWidth));
        }
        public static void ConsoleArt()
        {
            Console.Clear();
            Colorful.Console.ReplaceAllColorsWithDefaults();
            Colorful.Console.WriteWithGradient(@"
               **      ******** ********** *******     *******         **      **  **** 
              ****    **////// /////**/// /**////**   **/////**       /**     /** */// *
             **//**  /**           /**    /**   /**  **     //**      /**     /**/    /*
            **  //** /*********    /**    /*******  /**      /**      //**    **    *** 
           **********////////**    /**    /**///**  /**      /**       //**  **    *//  
          /**//////**       /**    /**    /**  //** //**     **         //****    *     
          /**     /** ********     /**    /**   //** //*******           //**    ******
          //      // ////////      //     //     //   ///////             //     ////// 
", Color.BlueViolet, Color.DeepPink, 5);
        }

        // Mod Functions
        public static void AstroMenu(bool state)
        {
            AstroLogs.Log("AstroMenu: Start processing.");

            CheckLethalCompany();
            try
            {
                if (state == true)
                {
                    AstroFileSystem.MoveFile($"{bepInExPath}\\core\\AstroMenu.dll", $"{pluginsPath}\\AstroMenu.dll");
                    AstroLogs.Log("AstroMenu enabled.");
                }
                else
                {
                    AstroFileSystem.MoveFile($"{pluginsPath}\\AstroMenu.dll", $"{bepInExPath}\\core\\AstroMenu.dll");
                    AstroLogs.Log("AstroMenu disabled.");
                }
            }
            catch (Exception ex)
            {
                ConsoleAnimation($"Error: {ex.Message}");
                AstroLogs.Log($"AstroMenu error: {ex.Message}");
                Thread.Sleep(2000);
            }

            AstroLogs.Log("AstroMenu: End processing.");
        }
        public static void BrutalCompany(bool state)
        {
            AstroLogs.Log("BrutalCompany: Start processing.");

            CheckLethalCompany();

            try
            {
                if (state == true)
                {
                    AstroFileSystem.MoveFile($"{bepInExPath}\\core\\BrutalCompanyPlus.dll", $"{pluginsPath}\\BrutalCompanyPlus.dll");
                    AstroLogs.Log("BrutalCompany enabled.");
                }
                else
                {
                    AstroFileSystem.MoveFile($"{pluginsPath}\\BrutalCompanyPlus.dll", $"{bepInExPath}\\core\\BrutalCompanyPlus.dll");
                    AstroLogs.Log("BrutalCompany disabled.");
                }
            }
            catch (Exception ex)
            {
                ConsoleAnimation($"Error: {ex.Message}");
                AstroLogs.Log($"BrutalCompany error: {ex.Message}");
                Thread.Sleep(2000);
            }

            AstroLogs.Log("BrutalCompany: End processing.");
        }
        public static void RichPresence(bool state)
        {
            AstroLogs.Log("RichPresence: Start processing.");

            CheckLethalCompany();
            var enabledPath = $"{pluginsPath}\\AstroRPC.dll";
            var disabledPath = $"{bepInExPath}\\core\\AstroRPC.dll";

            if (!AstroFileSystem.FileExists(enabledPath) || !AstroFileSystem.FileExists(disabledPath))
            {
                ConsoleAnimation("AstroRPC.dll is missing.");
                AstroLogs.Log("AstroRPC.dll is missing.");
                Thread.Sleep(2000);
                return;
            }

            try
            {
                if (state == true)
                {
                    // Enable
                    if (richState != "Enabled")
                    {
                        AstroFileSystem.MoveFile(disabledPath, enabledPath);
                        AstroLogs.Log("RichPresence enabled.");
                    }
                    else
                    {
                        ConsoleAnimation($"Rich Presence is already {richState}.");
                        AstroLogs.Log($"Rich Presence is already {richState}.");
                    }
                }
                else
                {
                    // Disable
                    if (richState != "Disabled")
                    {
                        AstroFileSystem.MoveFile(enabledPath, disabledPath);
                        AstroLogs.Log("RichPresence disabled.");
                    }
                    else
                    {
                        ConsoleAnimation($"Rich Presence is already {richState}.");
                        AstroLogs.Log($"Rich Presence is already {richState}.");
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleAnimation($"Error: {ex.Message}");
                AstroLogs.Log($"RichPresence error: {ex.Message}");
                Thread.Sleep(2000);
            }

            AstroLogs.Log("RichPresence: End processing.");
        }
        public static void RetroShading(bool state)
        {
            // Too lazy to make logs for this lol
            CheckLethalCompany();
            if (state == true)
            {
                if (CheckForExistingShaders() == true)
                {
                    SetColor(Color.DarkRed);
                    ConsoleAnimation("Existing Shaders Found. Please remove them before installing new shaders.");
                    ConsoleAnimation("Press any key to continue...");
                    Console.ReadKey();
                    ConsoleMain.AppHandler();
                }
                using (var client = server)
                {
                    SetColor(Color.Magenta);
                    ConsoleAnimation("Downloading...");
                    try
                    {
                        client.DownloadProgressChanged += (s, e) =>
                        {
                            Console.Title = $"ASTRO BOYZ! | Downloading... | {e.ProgressPercentage}% ({e.BytesReceived}/{e.TotalBytesToReceive})";
                        };
                        client.DownloadFileAsync(new Uri(files["reshade"]), $"{lethalCompanyPath}\\setup.exe");
                        while (client.IsBusy)
                        {
                            Thread.Sleep(500);
                        }
                        client.DownloadFileAsync(new Uri(files["shader"]), $"{lethalCompanyPath}\\RetroShader.ini");
                        while (client.IsBusy)
                        {
                            Thread.Sleep(500);
                        }
                        client.DownloadFileAsync(new Uri(files["instructions"]), $"{lethalCompanyPath}\\intructions.txt");
                    }
                    catch (Exception ex)
                    {
                        ConsoleAnimation($"Error Downloading Shaders: {ex.Message}");
                        Thread.Sleep(2500);
                        return;
                    }
                    ConsoleAnimation("Opening Setup...");
                    SetColor(Color.Magenta);
                    Process p1 = null;
                    Process p2 = null;
                    try
                    {
                        p1 = Process.Start($"{lethalCompanyPath}\\setup.exe");
                        p2 = Process.Start($"{lethalCompanyPath}\\intructions.txt");
                        SetForegroundWindow(p1.MainWindowHandle);
                        SetForegroundWindow(p2.MainWindowHandle);
                    }
                    catch (Exception ex)
                    {
                        SetColor(Color.DarkRed);
                        ConsoleAnimation($"Error Starting Processes: {ex.Message}");
                        ConsoleAnimation("Perhaps the file was corrupted?");
                        ConsoleAnimation("Press any key to continue...");
                        Console.ReadKey();
                    }
                    SetColor(Color.DarkRed);
                    ConsoleAnimation("Please follow the instructions in the window(s) that were opened!!!!");
                    while (p1.HasExited == false)
                    {
                        Console.Title = $"ASTRO BOYZ! | Waiting for Setup to Finish or Exit... | {p1.MainWindowTitle}";
                        Thread.Sleep(500);
                    }
                    if (AstroFileSystem.FileExists($"{lethalCompanyPath}\\ReShade.ini"))
                    {
                        ReplaceIniValue($"{lethalCompanyPath}\\ReShade.ini", "INPUT", "KeyOverlay", "73,1,0,0");
                    }
                    Thread.Sleep(2000);
                }
            }
            else
            {
                if (CheckForExistingShaders() == false)
                {
                    SetColor(Color.DarkRed);
                    ConsoleAnimation("No shaders found.");
                    ConsoleAnimation("Press any key to continue...");
                    Console.ReadKey();
                    ConsoleMain.AppHandler();
                    return;
                }
                else
                {
                    AstroFileSystem.DeleteDirectory(lethalCompanyPath + "\\reshade-shaders");
                    AstroFileSystem.DeleteFile(lethalCompanyPath + "\\dxgi.dll");
                    AstroFileSystem.DeleteFile(lethalCompanyPath + "\\ReShade.ini");
                    AstroFileSystem.DeleteFile(lethalCompanyPath + "\\ReShade.log");
                    AstroFileSystem.DeleteFile(lethalCompanyPath + "\\ReShadePreset.ini");
                    AstroFileSystem.DeleteFile(lethalCompanyPath + "\\setup.exe");
                    AstroFileSystem.DeleteFile(lethalCompanyPath + "\\RetroShader.ini");
                    AstroFileSystem.DeleteFile(lethalCompanyPath + "\\intructions.txt");
                    SetColor(Color.Green);
                    ConsoleAnimation("Shaders Removed!");
                    Thread.Sleep(2500);
                    ConsoleMain.AppHandler();
                }
            }
        }

        // Utility Functions
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
                    if (AstroFileSystem.DirectoryExists(potentialSteamPath))
                    {
                        libraryFolders.Add(potentialSteamPath);
                    }
                }
            }

            // Parse the libraryfolders.vdf file for additional library folders
            string libraryFoldersFile = Path.Combine(mainSteamPath, "steamapps", "libraryfolders.vdf");
            if (AstroFileSystem.FileExists(libraryFoldersFile))
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
        public static string GetSteamPath()
        {
            // Messy way of doing things, but it gets the job done
            string steamPath = null;
            string steamRegKey = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Valve\\Steam";
            string steamRegKey64 = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Valve\\Steam";
            string steamId = "HKEY_CURRENT_USER\\Software\\Valve\\Steam\\ActiveProcess";
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
            var idFound = Registry.GetValue(steamId, "ActiveUser", null);
            currentSteamId = idFound.ToString();
            return steamPath;
        }
        private static string ExtractValue(string line)
        {
            var match = Regex.Match(line, "\"[^\"]+\"\\s+\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : "";
        }
        public static string GetFullUserName()
        {
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

            if (windowsIdentity != null)
            {
                return windowsIdentity.Name;
            }

            return null;
        }
        public static void GetFolderPath()
        {
            try
            {
                AstroLogs.Log("Starting to get folder path.");

                currentConfig.customPath = null;
                currentConfig.Save();
                AstroLogs.Log("Current custom path reset and configuration saved.");

                // Main Prompt
                ConsoleAnimation("Example: C:\\Program Files (x86)\\Steam\\steamapps\\common\\Lethal Company");
                ConsoleAnimation("Please type a folder path or you can just press 'Enter' to select a folder.");
                string folderPath = Console.ReadLine();
                Console.WriteLine();
                AstroLogs.Log($"User entered folder path: {folderPath}");

                if (string.IsNullOrWhiteSpace(folderPath))
                {
                    using (var folderBrowserDialog = new FolderBrowserDialog())
                    {
                        DialogResult result = folderBrowserDialog.ShowDialog();

                        if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                        {
                            folderPath = folderBrowserDialog.SelectedPath;
                            AstroLogs.Log($"Folder path selected using browser dialog: {folderPath}");
                        }
                    }
                }
                else if (folderPath.Contains('"'))
                {
                    folderPath = folderPath.Split('"')[1];
                    AstroLogs.Log($"Folder path adjusted from user input: {folderPath}");
                }

                ConsoleAnimation(folderPath);
                ConsoleAnimation("Path Saved!");

                // Set Other Values to make sure it works
                lethalCompanyPath = folderPath;
                currentConfig.customPath = folderPath;
                currentConfig.Save();
                RefreshPath();
                AstroLogs.Log($"Path saved and set: {folderPath}");
            }
            catch (Exception ex)
            {
                AstroLogs.Log($"Error in GetFolderPath: {ex.Message}");
            }
        }
        public static void GetExtrasStates()
        {
            if (AstroFileSystem.FileExists($"{pluginsPath}\\AstroMenu.dll"))
            {
                astroState = "Enabled";
            }
            else
            {
                astroState = "Disabled";
            }
            if (AstroFileSystem.FileExists($"{pluginsPath}\\BrutalCompanyPlus.dll"))
            {
                brutalState = "Enabled";
            }
            else
            {
                brutalState = "Disabled";
            }
            if (AstroFileSystem.FileExists($"{pluginsPath}\\AstroRPC.dll"))
            {
                richState = "Enabled";
            }
            else
            {
                richState = "Disabled";
            }
            if (CheckForExistingShaders() == true)
            {
                retroState = "Installed";
            }
            else
            {
                retroState = "Not Installed";
            }
        }
        public static void GetInstalledModNames()
        {
            Console.Clear();
            if (AstroFileSystem.DirectoryExists(pluginsPath))
            {
                List<FileSystemInfo> items = new DirectoryInfo(pluginsPath).GetFileSystemInfos().ToList();

                // Separate files and folders
                var files = items.OfType<FileInfo>().ToList();
                var folders = items.OfType<DirectoryInfo>().ToList();

                // Group files by extension
                var groupedFiles = files.GroupBy(file => file.Extension.ToLower())
                                        .ToDictionary(group => group.Key, group => group.ToList());
                // Display grouped files in side-by-side rows of 2
                Console.SetWindowSize(100, 40);
                Console.BufferWidth = Console.WindowWidth;
                Console.BufferHeight = Console.WindowHeight;
                foreach (var fileGroup in groupedFiles)
                {
                    SetColor(Color.Cyan);
                    if (fileGroup.Key == "")
                    {
                        Console.WriteLine($"File Type: ???");
                    }
                    else
                    {
                        Console.WriteLine($"File Type: {fileGroup.Key}");
                    }
                    int count = 0;
                    SetColor(Color.LightGray);
                    foreach (var fileInfo in fileGroup.Value)
                    {
                        string truncatedName = TruncateFileName(fileInfo.Name, 25);
                        Console.Write($"{truncatedName,-30}");
                        count++;

                        if (count % 3 == 0)
                        {
                            Console.WriteLine();
                        }
                    }

                    // If there is an odd number of files in the group, add a newline
                    if (count % 3 != 0)
                    {
                        Console.WriteLine();
                    }

                    Console.WriteLine();
                }

                SetColor(Color.Cyan);
                Console.WriteLine("Folders in the directory:");

                foreach (var folderInfo in folders)
                {
                    SetColor(Color.LightGray);
                    Console.Write($"{folderInfo.Name,-40}");

                    if (folders.IndexOf(folderInfo) % 2 != 0)
                    {
                        Console.WriteLine();
                    }
                }
            }
            else
            {
                Console.WriteLine("Directory does not exist.");
            }
            string TruncateFileName(string fileName, int maxLength)
            {
                if (fileName.Length > maxLength)
                {
                    return fileName.Substring(0, maxLength - 3) + "...";
                }
                return fileName;
            }
        }
        public static void RefreshPath()
        {
            // Just to prevent something from breaking
            if (lethalCompanyPath != null)
            {
                bepInExPath = $"{lethalCompanyPath}\\BepInEx";
                pluginsPath = $"{bepInExPath}\\plugins";
            }
        }
        public static void OpenLethalCompanyFolder()
        {
            Process.Start(lethalCompanyPath);
        }
        public static void LaunchSteamGame(int game)
        {
            try
            {
                AstroLogs.Log($"Attempting to launch Steam game with ID: {game}");
                Process.Start("steam://rungameid/" + game);
                AstroLogs.Log("Steam game launched successfully.");
            }
            catch (Exception ex)
            {
                SetColor(Color.DarkRed);
                ConsoleAnimation($"Error Starting Steam game: {ex.Message}");
                AstroLogs.Log($"Error starting Steam game: {ex.Message}");
            }
        }
        public static void CloseSteamGame(int appId)
        {
            string steamUrl = $"steam://nav/games/details/{appId}";

            try
            {
                AstroLogs.Log($"Attempting to close Steam game with AppID: {appId}");
                // Open the Steam game's details page
                Process.Start(steamUrl);

                // Find the process associated with the game by its name
                Process[] processes = Process.GetProcessesByName("Lethal Company");
                foreach (Process process in processes)
                {
                    // Close the process
                    process.CloseMainWindow();
                    process.WaitForExit();

                    AstroLogs.Log($"Closed process: {process.ProcessName}");
                    // I could use Process.Kill but then data would not save properly.
                }
                AstroLogs.Log("All processes associated with the game have been closed.");
            }
            catch (Exception ex)
            {
                SetColor(Color.DarkRed);
                ConsoleAnimation($"Error closing Steam game: {ex.Message}");
                AstroLogs.Log($"Error closing Steam game: {ex.Message}");
            }
        }
        public static void ReplaceIniValue(string filePath, string section, string key, string newValue)
        {
            // Read all lines from the file
            List<string> lines = new List<string>(File.ReadAllLines(filePath));

            // Create a regular expression pattern to match the key within the specified section
            string pattern = $@"^\s*{key}\s*=\s*(.*)\s*$";

            // Iterate through the lines and find the key within the specified section
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim() == $"[{section}]")
                {
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        Match match = Regex.Match(lines[j], pattern);

                        if (match.Success)
                        {
                            // Replace the old value with the new value
                            lines[j] = $"{key}={newValue}";
                            break;
                        }
                        else if (lines[j].StartsWith("["))
                        {
                            // If a new section is encountered, break the inner loop
                            break;
                        }
                    }
                    break;
                }
            }

            // Write the modified lines back to the file
            File.WriteAllLines(filePath, lines);
        }

        // Checks
        public static async Task CheckDependencies()
        {
            try
            {
                AstroLogs.Log("Checking for missing dependencies.");

                if (!AstroFileSystem.DirectoryExists("MenuMusic"))
                {
                    Directory.CreateDirectory("MenuMusic");
                    AstroLogs.Log("MenuMusic directory created.");
                }

                var dependencyMissing = false;
                using (WebClient client = server)
                {
                    foreach (var dependency in dependencies)
                    {
                        if (!AstroFileSystem.FileExists(dependency.Key))
                        {
                            dependencyMissing = true;
                            Console.WriteLine($"Downloading Missing Dependency [{dependency.Key}]...");
                            AstroLogs.Log($"Downloading missing dependency: {dependency.Key}");

                            try
                            {
                                await client.DownloadFileTaskAsync(dependency.Value, dependency.Key);
                                AstroLogs.Log($"Successfully downloaded dependency: {dependency.Key}");
                            }
                            catch (Exception ex)
                            {
                                SetColor(Color.DarkRed);
                                Console.WriteLine($"Error downloading dependency, Please report to Astro: {ex.InnerException}");
                                AstroLogs.Log($"Error downloading dependency {dependency.Key}: {ex.InnerException}");

                                Thread.Sleep(10000);
                                Environment.Exit(0);
                            }
                        }
                    }
                }

                if (dependencyMissing)
                {
                    Console.WriteLine("All dependencies downloaded successfully.");
                    AstroLogs.Log("All missing dependencies downloaded successfully.");

                    Thread.Sleep(2000);
                    Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                    AstroLogs.Log("Restarting process due to new dependencies.");

                    Process.GetCurrentProcess().Kill();
                }
                else
                {
                    AstroLogs.Log("No missing dependencies found.");
                }
            }
            catch (Exception ex)
            {
                AstroLogs.Log($"Error in CheckDependencies: {ex.Message}");
            }
        }
        public static async Task CheckForUpdates()
        {
            try
            {
                AstroLogs.Log("Starting update check.");
                Colorful.Console.WriteWithGradient(@"
 **     ** *******  *******       **     ********** ******** *******  
/**    /**/**////**/**////**     ****   /////**/// /**///// /**////** 
/**    /**/**   /**/**    /**   **//**      /**    /**      /**   /** 
/**    /**/******* /**    /**  **  //**     /**    /******* /*******  
/**    /**/**////  /**    /** **********    /**    /**////  /**///**  
/**    /**/**      /**    ** /**//////**    /**    /**      /**  //** 
//******* /**      /*******  /**     /**    /**    /********/**   //**
 ///////  //       ///////   //      //     //     //////// //     // 
", Color.BlueViolet, Color.HotPink, 5);
                Console.SetWindowSize(100, 30);
                Console.SetBufferSize(100, 30);
                ConsoleAnimation($"Current Version: {currentVersion}");
                AstroLogs.Log($"Current version: {currentVersion}");

                ConsoleAnimation("Checking for updates...");
                ConsoleAnimation($"Latest Version: {AstroServer.latestVersion}");
                AstroLogs.Log($"Latest available version: {AstroServer.latestVersion}");

                if (currentVersion < AstroServer.latestVersion)
                {
                    AstroLogs.Log("Update available.");

                    if (currentConfig.autoUpdate == true)
                    {
                        ConsoleAnimation("Updating automatically...");
                        AstroLogs.Log("Automatic update initiated.");
                        await Task.Delay(1000);
                        ConsoleMain.ForceAppUpdate();
                    }
                    else
                    {
                        ConsoleAnimation("Update Available! Would you like to update now?");
                        GenerateOption(new MenuOption()
                        {
                            option = "Yes",
                            identity = "0",
                            matchMenu = false,
                            newLine = true
                        });
                        GenerateOption(new MenuOption()
                        {
                            option = "No",
                            identity = "1",
                            matchMenu = false,
                            newLine = true
                        });

                        var currOption = Console.ReadLine();
                        Console.WriteLine();
                        AstroLogs.Log($"User selected option: {currOption}");

                        switch (currOption)
                        {
                            case "0":
                                ConsoleAnimation("Updating..");
                                AstroLogs.Log("User initiated update.");
                                await Task.Delay(1000);
                                ConsoleMain.ForceAppUpdate();
                                break;
                            case "1":
                                ConsoleAnimation("Update Skipped.");
                                AstroLogs.Log("User skipped update.");
                                await Task.Delay(1000);
                                break;
                            default:
                                SetColor(Color.DarkRed);
                                ConsoleAnimation("Invalid Option. Please try again.");
                                AstroLogs.Log("User entered invalid option for update.");
                                break;
                        }
                    }
                }
                else if (currentVersion > AstroServer.latestVersion)
                {
                    ConsoleAnimation("Time Traveler. You have a newer version.");
                    AstroLogs.Log("Current version is newer than the latest version.");
                    await Task.Delay(2000);
                }
                else
                {
                    ConsoleAnimation("No Updates Available!");
                    AstroLogs.Log("No updates available.");
                    await Task.Delay(2000);
                }

                Colorful.Console.ReplaceAllColorsWithDefaults();
            }
            catch (Exception ex)
            {
                AstroLogs.Log($"Error in CheckForUpdates: {ex.Message}");
            }
        }
        public static void CheckCurrentStates()
        {
            if (AstroFileSystem.FileExists($"{pluginsPath}\\AstroMenu.dll"))
            {
                astroState = "Enabled";
            }
            else
            {
                astroState = "Disabled";
            }
            if (AstroFileSystem.FileExists($"{pluginsPath}\\BrutalCompanyPlus.dll"))
            {
                brutalState = "Enabled";
            }
            else
            {
                brutalState = "Disabled";
            }
            if (CheckForExistingShaders() == true)
            {
                retroState = "Installed";
            }
            else
            {
                retroState = "Not Installed";
            }
        }
        public static void CheckSteamInstallData()
        {
            try
            {
                AstroLogs.Log("Starting to check Steam install data.");

                Colorful.Console.Clear();

                var allLibraryFolders = GetAllSteamLibraryFolders();
                AstroLogs.Log($"Found {allLibraryFolders.Count} Steam library folders.");

                foreach (var steamAppsFolder in allLibraryFolders)
                {
                    string[] appManifestFiles = Directory.GetFiles(steamAppsFolder, "appmanifest_*.acf");
                    AstroLogs.Log($"Found {appManifestFiles.Length} app manifest files in {steamAppsFolder}.");

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
                            Colorful.StyleSheet styleSheet = new Colorful.StyleSheet(Color.Gray);
                            styleSheet.AddStyle($"{appName}[a-z]*", Color.BlueViolet);

                            Colorful.Console.WriteLineStyled($"Found Game: {appName}", styleSheet);
                            AstroLogs.Log($"Found game: {appName} at {fullPath}");
                        }

                        if (appName == "Lethal Company")
                        {
                            lethalCompanyPath = fullPath; // Make sure lethalCompanyPath is declared somewhere
                            AstroLogs.Log("Lethal Company path set.");
                        }
                    }
                }

                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                AstroLogs.Log($"Error in CheckSteamInstallData: {ex.Message}");
            }
        }

        public static void CheckLethalCompany()
        {
            Process[] lc = Process.GetProcessesByName("Lethal Company");
            if (lc.Length != 0)
            {
                SetColor(Color.DarkRed);
                Console.Clear();
                ConsoleAnimation("Lethal Company is currently running. Please close the game before continuing.\n");
                ConsoleAnimation("Press any key to continue...");
                Console.ReadKey();
                CheckLethalCompany();
            }
        }
        public static bool CheckForExistingMods()
        {
            if (AstroFileSystem.DirectoryExists(bepInExPath) || AstroFileSystem.DirectoryExists($"{lethalCompanyPath}\\winhttp.dll") || AstroFileSystem.DirectoryExists($"{lethalCompanyPath}\\doorstop_config.ini"))
            {
                return true;
            }
            return false;
        }
        public static bool CheckForExistingShaders()
        {
            var DetectedFiles = new List<string>()
            {
                $"{lethalCompanyPath}\\dxgi.dll",
                $"{lethalCompanyPath}\\ReShade.ini",
                $"{lethalCompanyPath}\\ReShade.log",
                $"{lethalCompanyPath}\\ReShadePreset.ini",
                $"{lethalCompanyPath}\\RetroShader.ini",
                $"{lethalCompanyPath}\\setup.exe",
            };
            if (AstroFileSystem.DirectoryExists(lethalCompanyPath + "\\reshade-shaders"))
            {
                return true;
            }
            if (DetectedFiles.Any(AstroFileSystem.FileExists))
            {
                return true;
            }
            return false;
        }

        // Misc Functions
        public static void ChangeLog()
        {
            Colorful.Console.ReplaceAllColorsWithDefaults();
            Console.Clear();
            Colorful.Console.WriteWithGradient(@"
               **      ******** ********** *******     *******         **      **  **** 
              ****    **////// /////**/// /**////**   **/////**       /**     /** */// *
             **//**  /**           /**    /**   /**  **     //**      /**     /**/    /*
            **  //** /*********    /**    /*******  /**      /**      //**    **    *** 
           **********////////**    /**    /**///**  /**      /**       //**  **    *//  
          /**//////**       /**    /**    /**  //** //**     **         //****    *     
          /**     /** ********     /**    /**   //** //*******           //**    ******
          //      // ////////      //     //     //   ///////             //     ////// 
", Color.BlueViolet, Color.DeepPink, 5);
            Colorful.Console.WriteWithGradient(AstroServer.currentChangelog, Color.BlueViolet, Color.DeepPink, 5);
            Console.SetWindowSize(100, 30);
            Console.SetBufferSize(100, 30);
            Console.WriteLine();
            ConsoleAnimation("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
        public static void GenerateMenu()
        {
            ConsoleArt();
            Console.SetWindowSize(100, 30);
            Console.SetBufferSize(100, 30);
            SetColor(Color.HotPink);
            Console.WriteLine($"                                                                                 {currentVersion}");
            SetColor(Color.BlueViolet);
            ConsoleAnimation("\nPlease type the number of the option you would like.");
            Console.WriteLine();
            Console.WriteLine("╔════ Mods ══════════════════════════════════╗");
            GenerateOption(new MenuOption()
            {
                option = "View Installed Mods",
                identity = "0",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true,
            });
            GenerateOption(new MenuOption()
            {
                option = "Install Mods",
                identity = "1",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new MenuOption()
            {
                option = "Remove Mods",
                identity = "2",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new MenuOption()
            {
                option = "Extra Mods",
                identity = "3",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            SetColor(Color.BlueViolet);
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.WriteLine("╔════ Util ══════════════════════════════════╗");
            GenerateOption(new MenuOption()
            {
                option = "Start / Stop Lethal Company",
                identity = "4",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new MenuOption()
            {
                option = "Open Lethal Company Folder",
                identity = "5",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.WriteLine("╔════ App ═══════════════════════════════════╗");
            GenerateOption(new MenuOption()
            {
                option = "Force Update Astro Boyz",
                identity = "6",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new MenuOption()
            {
                option = "View Change Log",
                identity = "7",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new MenuOption()
            {
                option = "Settings",
                identity = "8",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            Console.WriteLine("╚════════════════════════════════════════════╝\n");

        }
        public static void ExtrasMenu()
        {
            GetExtrasStates();
            ConsoleArt();
            Console.SetWindowSize(100, 30);
            Console.SetBufferSize(100, 30);
            SetColor(Color.HotPink);
            Console.WriteLine($"                                                                                 {currentVersion}");
            SetColor(Color.BlueViolet);
            Console.WriteLine($"Astro Menu: {astroState}");
            Console.WriteLine($"Brutal Company: {brutalState}");
            Console.WriteLine($"Retro Shading: {retroState}");
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════╗");
            GenerateOption(new MenuOption()
            {
                option = "Astro Menu",
                identity = "0",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true,
                warning = " (CONSIDERED CHEATS)"
            });
            GenerateOption(new MenuOption()
            {
                option = "Brutal Company",
                identity = "1",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true,
                warning = " (HOST ONLY)",
            });
            GenerateOption(new MenuOption()
            {
                option = "Retro Shading",
                identity = "2",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new MenuOption()
            {
                option = "Back",
                identity = "9",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            Console.WriteLine("╚════════════════════════════════════════════╝\n\n");
            var currOption = Console.ReadLine(); Console.WriteLine();
            switch (currOption)
            {
                case "0":
                    ConsoleAnimation("Would you like to enable or disable Astro Menu?");
                    GenerateOption(new MenuOption() { option = "Enable", identity = "0", color = Color.DarkGreen, matchMenu = false, newLine = true });
                    GenerateOption(new MenuOption() { option = "Disable", identity = "1", color = Color.DarkRed, matchMenu = false, newLine = true });
                    var currOption2 = Console.ReadLine(); Console.WriteLine();
                    switch (currOption2)
                    {
                        case "0":
                            AstroMenu(true);
                            ConsoleAnimation("Astro Menu Enabled!");
                            break;
                        case "1":
                            AstroMenu(false);
                            ConsoleAnimation("Astro Menu Disabled!");
                            break;
                        default:
                            SetColor(Color.DarkRed);
                            ConsoleAnimation("Invalid Option.");
                            break;
                    }
                    Thread.Sleep(2000);
                    break;
                case "1":
                    ConsoleAnimation("Would you like to enable or disable Brutal Company?");
                    GenerateOption(new MenuOption() { option = "Enable", identity = "0", color = Color.DarkGreen, matchMenu = false, newLine = true });
                    GenerateOption(new MenuOption() { option = "Disable", identity = "1", color = Color.DarkRed, matchMenu = false, newLine = true });
                    var currOption3 = Console.ReadLine(); Console.WriteLine();
                    switch (currOption3)
                    {
                        case "0":
                            BrutalCompany(true);
                            ConsoleAnimation("Brutal Company Enabled!");
                            break;
                        case "1":
                            BrutalCompany(false);
                            ConsoleAnimation("Brutal Company Disabled!");
                            break;
                        default:
                            SetColor(Color.DarkRed);
                            ConsoleAnimation("Invalid Option.");
                            break;
                    }
                    Thread.Sleep(2000);
                    break;
                case "2":
                    ConsoleAnimation("Would you like to Install or Remove Retro Shading?");
                    GenerateOption(new MenuOption()
                    {
                        option = "Install",
                        identity = "0",
                        color = Color.DarkGreen,
                        matchMenu = false,
                        newLine = true
                    });
                    GenerateOption(new MenuOption()
                    {
                        option = "Remove",
                        identity = "1",
                        color = Color.DarkRed,
                        matchMenu = false,
                        newLine = true
                    });
                    var currOption5 = Console.ReadLine(); Console.WriteLine();
                    switch (currOption5)
                    {
                        case "0":
                            RetroShading(true);
                            break;
                        case "1":
                            RetroShading(false);
                            break;
                        default:
                            SetColor(Color.DarkRed);
                            ConsoleAnimation("Invalid Option.");
                            break;
                    }
                    Thread.Sleep(2000);
                    break;
                case "9":
                    Console.Clear();
                    ConsoleMain.AppHandler();
                    break;
                default:
                    SetColor(Color.DarkRed);
                    ConsoleAnimation("Invalid Option.");
                    Thread.Sleep(2000);
                    break;
            }
        }
        public static void SettingsMenu()
        {
            ConsoleArt();
            Console.SetWindowSize(100, 30);
            Console.SetBufferSize(100, 30);
            SetColor(Color.HotPink);
            Console.WriteLine($"                                                                                 {currentVersion}");
            SetColor(Color.BlueViolet);
            Console.WriteLine($"Menu Music: {currentConfig.menuMusic}");
            Console.WriteLine($"Auto Update: {currentConfig.autoUpdate}");
            Console.WriteLine($"Custom Path: {currentConfig.customPath ?? "Steam"}");
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════╗");
            GenerateOption(new MenuOption()
            {
                option = "Auto Update",
                identity = "0",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new MenuOption()
            {
                option = "Allow Menu Music",
                identity = "1",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new MenuOption()
            {
                option = "Priority Song",
                identity = "2",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new MenuOption()
            {
                option = "Back",
                identity = "9",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            Console.WriteLine("╚════════════════════════════════════════════╝\n");
            var currOption = Console.ReadLine(); Console.WriteLine();
            switch (currOption)
            {
                case "1":
                    ConsoleAnimation("Would you like to enable or disable Menu Music?");
                    GenerateOption(new MenuOption() { option = "Enable", identity = "0", matchMenu = false, newLine = true });
                    GenerateOption(new MenuOption() { option = "Disable", identity = "1", matchMenu = false, newLine = true });
                    var currOption2 = Console.ReadLine(); Console.WriteLine();
                    switch (currOption2)
                    {
                        case "0":
                            currentConfig.menuMusic = true;
                            currentConfig.Save();
                            ConsoleAnimation("Music Enabled!");
                            break;
                        case "1":
                            currentConfig.menuMusic = false;
                            currentConfig.Save();
                            ConsoleAnimation("Music Disabled!");
                            break;
                        default:
                            SetColor(Color.DarkRed);
                            ConsoleAnimation("Invalid Option. Please try again.");
                            break;
                    }
                    Thread.Sleep(1000);
                    break;
                case "0":
                    ConsoleAnimation("Would you like to enable or disable Auto Update?");
                    GenerateOption(new MenuOption() { option = "Enable", identity = "0", matchMenu = false, newLine = true });
                    GenerateOption(new MenuOption() { option = "Disable", identity = "1", matchMenu = false, newLine = true });
                    var currOption3 = Console.ReadLine(); Console.WriteLine();
                    switch (currOption3)
                    {
                        case "0":
                            currentConfig.autoUpdate = true;
                            currentConfig.Save();
                            ConsoleAnimation("Auto Update Enabled!");
                            break;
                        case "1":
                            currentConfig.autoUpdate = false;
                            currentConfig.Save();
                            ConsoleAnimation("Auto Update Disabled!");
                            break;
                        case "9":
                            break;
                        default:
                            SetColor(Color.DarkRed);
                            ConsoleAnimation("Invalid Option. Please try again.");
                            break;
                    }
                    Thread.Sleep(1000);
                    break;
                case "2":
                    MusicMenu();
                    break;
                case "9":
                    Console.Clear();
                    ConsoleMain.AppHandler();
                    break;
                default:
                    SetColor(Color.DarkRed);
                    ConsoleAnimation("Invalid Option. Please try again.");
                    Thread.Sleep(1000);
                    break;
            }

        }
        public static void MusicMenu()
        {
            ConsoleArt();
            SetColor(Color.BlueViolet);
            Console.SetWindowSize(100, 30);
            Console.SetBufferSize(100, 30);
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════╗");
            var i = 0;
            foreach (var item in ConsoleMain.music)
            {
                i++;
                GenerateOption(new MenuOption()
                {
                    option = item.Key, // Use the Value of the Dictionary pair
                    identity = i.ToString(), // Use the Key of the Dictionary pair
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
            }
            Console.WriteLine("╚════════════════════════════════════════════╝\n");
            var currOption = Console.ReadLine(); Console.WriteLine();
            var tempValue = ConsoleMain.musicIndex[currOption];
            ConsoleAnimation($"Playing {tempValue}...");
            currentConfig.priotitySong = tempValue;
            currentConfig.Save();
            ConsoleMain.prioritySong = true;
            Thread.Sleep(1000);
        }
        public static void GenerateOption(MenuOption options)
        {
            // lol
            var Option = options.option ?? "";
            var identity = options.identity ?? "";
            var color = Color.BlueViolet;
            var matchMenu = options.matchMenu ?? true;
            var newLine = options.newLine ?? true;
            var warning = options.warning ?? "";
            var warningColor = Color.Gray;

            // set cursor to console center
            var originalConsoleColor = Colorful.Console.ForegroundColor;
            if (matchMenu == true)
            {
                Console.Write("║");
            }
            SetColor(color);
            Colorful.Console.Write(" [ ");
            SetColor(Color.LightGray);
            Colorful.Console.Write(identity);
            SetColor(color);
            Colorful.Console.Write(" ] ");
            SetColor(Color.LightGray);
            if (newLine == true)
            {
                if (matchMenu == true)
                {
                    var old = Console.CursorLeft;
                    Colorful.Console.SetCursorPosition(45, Console.CursorTop);
                    SetColor(color);
                    Colorful.Console.Write("║");
                    SetColor(Color.LightGray);
                    Colorful.Console.SetCursorPosition(old, Console.CursorTop);
                }
                Colorful.Console.Write(Option);
                SetColor(warningColor);
                Colorful.Console.Write(warning + "\n");
            }
            else
            {
                if (matchMenu == true)
                {
                    var old = Console.CursorLeft;
                    Colorful.Console.SetCursorPosition(45, Console.CursorTop);
                    SetColor(color);
                    Colorful.Console.Write("║");
                    SetColor(Color.LightGray);
                    Colorful.Console.SetCursorPosition(old, Console.CursorTop);
                }
                Colorful.Console.Write(Option);
                SetColor(warningColor);
                Colorful.Console.Write(warning);
            }
            SetColor(originalConsoleColor);
        }
    }
}
