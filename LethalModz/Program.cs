using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Mime;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows.Forms;
internal class Program
{
    [DllImport("user32.dll")]
    public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();
    [STAThread]
    private static void Main(string[] args)
    {
        // Begin Application
        Console.Title = $"Astro Boyz {Utilities.currentVersion}";
        Console.CursorVisible = false;
        Console.SetWindowSize(100, 30);
        Utilities.lethalCompanyPath = Utilities.GetSteamPath() + "\\steamapps\\common\\Lethal Company";
        Utilities.StopResizing();
        Utilities.CheckLethalCompany();
        Utilities.ReloadUpdater();
        Utilities.CheckCustomPath();
        Utilities.RefreshPath();

        // Make Sure Path Exists
        if (Directory.Exists(Utilities.lethalCompanyPath))
        {
            Utilities.SetColor(ConsoleColor.Cyan);
            try
            {
                Console.Title = $"ASTRO BOYZ! | {Utilities.lethalCompanyPath.Split(new string[] { "Files " }, StringSplitOptions.None)[1]} |";
            }
            catch
            {
                Console.Title = $"ASTRO BOYZ! | {Utilities.lethalCompanyPath} |";
            }
        }
        else
        {
            Utilities.SetColor(ConsoleColor.DarkRed);
            Console.Title = "ASTRO BOYZ! | Lethal Company Not Found";
            Utilities.ConsoleAnimation("Error: Lethal Company Not Found.");
            Utilities.ConsoleAnimation("If you believe this was a mistake, Contact VoidedJayz / Astro Boyz for Support.");
            Utilities.ConsoleAnimation("If you recently modified the Lethal Company Location, delete 'cpath.txt'.");
            Utilities.ConsoleAnimation("Alternatively, you can select the install path for Lethal Company.");
            Utilities.GenerateOption(new AstroOption()
            {
                option = "Exit.",
                identity = "1",
                color = ConsoleColor.DarkRed,
                matchMenu = true,
                newLine = true
            });
            Utilities.GenerateOption(new AstroOption()
            {
                option = "Select Install Path.",
                identity = "2",
                color = ConsoleColor.DarkRed,
                matchMenu = true,
                newLine = true
            });
            var currOption = Console.ReadLine();
            switch (currOption)
            {
                case "1":
                    Environment.Exit(0);
                    break;
                case "2":
                    string selectedFolder = Utilities.ShowFolderDialog();
                    if (!string.IsNullOrEmpty(selectedFolder))
                    {
                        if (File.Exists($"{selectedFolder}\\Lethal Company.exe"))
                        {
                            Utilities.lethalCompanyPath = selectedFolder;
                            Utilities.RefreshPath();
                            Utilities.SetColor(ConsoleColor.Cyan);
                            Utilities.ConsoleAnimation($"Custom Path: {Utilities.lethalCompanyPath}");
                            File.WriteAllText("cpath.txt", Directory.GetCurrentDirectory());
                        }
                        else
                        {
                            Utilities.ConsoleAnimation("Error: Lethal Company.exe Not Found.");
                            Utilities.ConsoleAnimation("Press any button to exit... :(");
                            Console.ReadLine();
                            Environment.Exit(0);
                        }
                    }
                    else
                    {
                        Utilities.ConsoleAnimation("Operation Canceled, Press any button to exit... :(");
                        Console.ReadLine();
                        Environment.Exit(0);
                    }
                    break;
                default:
                    Utilities.ConsoleAnimation("Invalid Option.");
                    Utilities.ConsoleAnimation("Press any button to exit... :(");
                    Console.ReadLine();
                    Environment.Exit(0);
                    break;
            }
        }

        // Begin Application Loop
        Utilities.AppHandler();
    }

    private class Utilities
    {
        // Variables
        public static string currentVersion = "2.0.0";
        public static string lethalCompanyPath = null;
        public static string currentSteamId = null;
        public static string bepInExPath = $"{lethalCompanyPath}\\BepInEx";
        public static string pluginsPath = $"{bepInExPath}\\plugins";
        public static string modzUrl = "https://cdn.astroswrld.club/secure/Files/modpack.zip";
        public static string reshadeUrl = "https://cdn.astroswrld.club/secure/Files/reshade.zip";
        public static WebClient server = new WebClient();

        // Console Functions
        public static void ConsoleAnimation(string input)
        {
            // Slowly Type Out Text
            var chars = input.ToCharArray();
            foreach (var letter in chars)
            {
                Console.Write(letter);
                Thread.Sleep(12);
            }
            Console.WriteLine();
        }
        public static void SetColor(ConsoleColor color)
        {
            // Why tf did I make this? Fucking useless, just to look cleaner ig
            Console.ForegroundColor = color;
        }
        public static void CenterText(string text)
        {
            // Center the input text on the console
            Console.WriteLine(text.PadLeft((Console.WindowWidth / 2) + (text.Length / 2)).PadRight(Console.WindowWidth));
        }

        // Application Functions
        public static void AstroMenu(bool state)
        {
            try
            {
                if (state == true)
                {
                    // Enable
                    File.Move($"{bepInExPath}\\core\\AstroMenu.dll", $"{pluginsPath}\\AstroMenu.dll");
                }
                else
                {
                    // Remove
                    File.Move($"{pluginsPath}\\AstroMenu.dll", $"{bepInExPath}\\core\\AstroMenu.dll");
                }
            }
            catch (Exception ex)
            {
                ConsoleAnimation($"Error: {ex.Message}");
                Thread.Sleep(2000);
            }
        }
        public static void BrutalCompany(bool state)
        {
            try
            {
                if (state == true)
                {
                    // Enable
                    File.Move($"{bepInExPath}\\core\\BrutalCompanyPlus.dll", $"{pluginsPath}\\BrutalCompanyPlus.dll");
                }
                else
                {
                    // Remove
                    File.Move($"{pluginsPath}\\BrutalCompanyPlus.dll", $"{bepInExPath}\\core\\BrutalCompanyPlus.dll");
                }
            }
            catch (Exception ex)
            {
                ConsoleAnimation($"Error: {ex.Message}");
                Thread.Sleep(2000);
            }   
        }
        public static void RichPresence(bool state)
        {
            try
            {
                if (state == true)
                {
                    // Enable
                    File.Move($"{bepInExPath}\\core\\LethalCompanyPresence.dll", $"{lethalCompanyPath}\\MLLoader\\Mods\\LethalCompanyPresence.dll");
                }
                else
                {
                    // Remove
                    File.Move($"{lethalCompanyPath}\\MLLoader\\Mods\\LethalCompanyPresence.dll", $"{bepInExPath}\\core\\LethalCompanyPresence.dll");
                }
            }
            catch (Exception ex)
            {
                ConsoleAnimation($"Error: {ex.Message}");
                Thread.Sleep(2000);
            }
        }
        public static void RetroShading(bool state)
        {
            if (state == true)
            {
                if (CheckForExistingShaders() == true)
                {
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation("Existing Shaders Found. Please remove them before installing new shaders.");
                    ConsoleAnimation("Press any key to continue...");
                    Console.ReadLine();
                    AppHandler();
                    return;
                }
                using (var client = server)
                {
                    SetColor(ConsoleColor.Magenta);
                    ConsoleAnimation("Downloading Zip...");
                    try
                    {
                        client.DownloadProgressChanged += (s, e) =>
                        {
                            Console.Title = $"ASTRO BOYZ! | Downloading... | {e.ProgressPercentage}% ({e.BytesReceived}/{e.TotalBytesToReceive})";
                        };
                        client.DownloadFileAsync(new Uri(GenerateSecureDownload(reshadeUrl, 30)), $"{lethalCompanyPath}\\temp_astro.zip");
                    }
                    catch (Exception ex)
                    {
                        ConsoleAnimation($"Error Downloading Mods: {ex.Message}");
                        Thread.Sleep(2500);
                        return;
                    }
                    while (client.IsBusy)
                    {
                        Thread.Sleep(500);
                    }
                    ConsoleAnimation("Zip Download Complete!");
                    ConsoleAnimation("Opening Zip...");
                    SetColor(ConsoleColor.Magenta);
                    try
                    {
                        using (ZipArchive archive = ZipFile.OpenRead($"{lethalCompanyPath}\\temp_astro.zip"))
                        {
                            using (var progress = new ProgressBar())
                            {
                                for (int i = 0; i <= archive.Entries.Count; i++)
                                {
                                    progress.Report((double)i / archive.Entries.Count);
                                    Console.Title = $"ASTRO BOYZ! | Installing Reshade... | {i}/{archive.Entries.Count}";
                                    Thread.Sleep(1);
                                }
                            }

                        }
                        Console.Write($"                                                                    ");
                        Console.SetCursorPosition(0, Console.CursorTop);
                        SetColor(ConsoleColor.Cyan);
                        ConsoleAnimation("Extracting Files...");
                        ZipFile.ExtractToDirectory($"{lethalCompanyPath}\\temp_astro.zip", $"{lethalCompanyPath}");
                        SetColor(ConsoleColor.Green);
                        ConsoleAnimation("Finished!");
                        File.Delete($"{lethalCompanyPath}\\temp_astro.zip");
                        Thread.Sleep(2500);
                    }
                    catch (Exception ex)
                    {
                        SetColor(ConsoleColor.DarkRed);
                        ConsoleAnimation($"Error Extracting Files: {ex.Message}");
                        ConsoleAnimation("Perhaps the file was corrupted?");
                        ConsoleAnimation("Press any key to continue...");
                        Console.ReadLine();
                    }
                }
            }
            else
            {
                if (CheckForExistingShaders() == false)
                {
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation("No shaders found.");
                    ConsoleAnimation("Press any key to continue...");
                    Console.ReadLine();
                    AppHandler();
                    return;
                }
                else
                {
                    Directory.Delete(lethalCompanyPath + "\\reshade-shaders", true);
                    File.Delete(lethalCompanyPath + "\\dxgi.dll");
                    File.Delete(lethalCompanyPath + "\\ReShade.ini");
                    File.Delete(lethalCompanyPath + "\\ReShade.log");
                    File.Delete(lethalCompanyPath + "\\ReShadePreset.ini");
                    SetColor(ConsoleColor.Green);
                    ConsoleAnimation("Shaders Removed!");
                    Thread.Sleep(2500);
                    AppHandler();
                }
            }
        }
        public static void AppHandler()
        {
            Console.Clear();
            try
            {
                // Console Title Displays Lethal Company Location
                Console.Title = $"Astro Boyz | {Utilities.lethalCompanyPath.Split(new string[] { "Files " }, StringSplitOptions.None)[1]} |";
            }
            catch
            {
                Console.Title = $"Astro Boyz | {Utilities.lethalCompanyPath} |";
            }
            // Menu Options
            GenerateMenu();

            // Massive fucking switch statement
            var currOption = Console.ReadLine();
            if (currOption != null)
            {
                switch (currOption)
                {
                    case "0":
                        GetInstalledModNames();
                        ConsoleAnimation("Press any key to continue...");
                        Console.ReadLine();
                        break;
                    case "1":
                        SetColor(ConsoleColor.Green);
                        ConsoleAnimation("Starting Mod Installer...");
                        InstallModz();
                        break;
                    case "2":
                        SetColor(ConsoleColor.DarkRed);
                        ConsoleAnimation("Starting Mod Remover...");
                        RemoveModz();
                        break;
                    case "3":
                        ExtrasMenu();
                        break;
                    case "4":
                        SetColor(ConsoleColor.Cyan);
                        ConsoleAnimation("Would you like to Start or Stop Lethal Company?");
                        GenerateOption(new AstroOption()
                        {
                            option = "Start",
                            identity = "0",
                            color = ConsoleColor.DarkGreen,
                            matchMenu = false,
                            newLine = true
                        });
                        GenerateOption(new AstroOption()
                        {
                            option = "Stop",
                            identity = "1",
                            color = ConsoleColor.DarkRed,
                            matchMenu = false,
                            newLine = true
                        });
                        var currOption2 = Console.ReadLine();
                        switch (currOption2)
                        {
                            case "0":
                                SetColor(ConsoleColor.Cyan);
                                ConsoleAnimation("Starting...");
                                LaunchSteamGame(1966720);
                                Thread.Sleep(1000);
                                Console.Clear();
                                break;
                            case "1":
                                SetColor(ConsoleColor.Cyan);
                                ConsoleAnimation("Closing...");
                                CloseSteamGame(1966720);
                                Thread.Sleep(1000);
                                Console.Clear();
                                break;
                            default:
                                SetColor(ConsoleColor.DarkRed);
                                ConsoleAnimation("Invalid Option. Please try again.");
                                break;
                        }
                        Thread.Sleep(1000);
                        Console.Clear();
                        break;
                    case "5":
                        SetColor(ConsoleColor.Cyan);
                        ConsoleAnimation("Opening...");
                        OpenLethalCompanyFolder();
                        Thread.Sleep(1000);
                        Console.Clear();
                        break;
                    case "6":
                        SetColor(ConsoleColor.Cyan);
                        ForceAppUpdate();
                        Environment.Exit(0);
                        break;
                    case "7":
                        SetColor(ConsoleColor.Cyan);
                        ConsoleAnimation("Removing...");
                        if (File.Exists("cpath.txt"))
                        {
                            SetColor(ConsoleColor.Green);
                            File.Delete("cpath.txt");
                            ConsoleAnimation("Custom Path Removed!");
                        }
                        else
                        {
                            SetColor(ConsoleColor.DarkRed);
                            ConsoleAnimation("Error: Custom Path Not Found.");
                        }
                        Thread.Sleep(1000);
                        Console.Clear();
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        ConsoleAnimation("Invalid Option. Please try again.");
                        break;
                }
            }
            AppHandler();
        }
        public static void InstallModz()
        {
            if (CheckForExistingMods() == true)
            {
                SetColor(ConsoleColor.DarkRed);
                ConsoleAnimation("Existing Mods Found. Please remove them before installing new mods.");
                ConsoleAnimation("Press any key to continue...");
                Console.ReadLine();
                AppHandler();
            }
            using (var client = server)
            {
                // More spaghetti code yayyyyy
                SetColor(ConsoleColor.Magenta);
                ConsoleAnimation("Downloading Zip...");
                try
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        Console.Title = $"ASTRO BOYZ! | Downloading... | {e.ProgressPercentage}% ({e.BytesReceived}/{e.TotalBytesToReceive})";
                    };
                    client.DownloadFileAsync(new Uri(GenerateSecureDownload(modzUrl, 30)), $"{lethalCompanyPath}\\temp_astro.zip");
                }
                catch (Exception ex)
                {
                    ConsoleAnimation($"Error Downloading Mods: {ex.Message}");
                    Thread.Sleep(2500);
                    return;
                }
                while (client.IsBusy)
                {
                    Thread.Sleep(500);
                }
                ConsoleAnimation("Zip Download Complete!");
                ConsoleAnimation("Opening Zip...");
                SetColor(ConsoleColor.Magenta);
                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead($"{lethalCompanyPath}\\temp_astro.zip"))
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
                    SetColor(ConsoleColor.Cyan);
                    ConsoleAnimation("Extracting Files...");
                    ZipFile.ExtractToDirectory($"{lethalCompanyPath}\\temp_astro.zip", $"{lethalCompanyPath}");
                    SetColor(ConsoleColor.Green);
                    ConsoleAnimation("Finished!");
                    File.Delete($"{lethalCompanyPath}\\temp_astro.zip");
                    Thread.Sleep(2500);
                }
                catch (Exception ex)
                {
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation($"Error Extracting Files: {ex.Message}");
                    ConsoleAnimation("Perhaps the file was corrupted?");
                    ConsoleAnimation("Press any key to continue...");
                    Console.ReadLine();
                }
            }
            Console.Clear();
            AppHandler();
        }
        public static void RemoveModz()
        {
            // Check For Currently Installed Modz
            if (CheckForExistingMods() == true)
            {
                SetColor(ConsoleColor.Cyan);
                try
                {
                    Directory.Delete(bepInExPath, true);
                }
                catch (Exception ex)
                {
                    ConsoleAnimation($"Error Deleting BepInEx Folder: {ex.InnerException}");
                }
                try
                {
                    Directory.Delete(lethalCompanyPath + "\\MLLoader", true);
                }
                catch (Exception ex)
                {
                    ConsoleAnimation($"Error Deleting MLLoader Folder: {ex.InnerException}");
                }
                try
                {
                    File.Delete($"{lethalCompanyPath}\\doorstop_config.ini");
                }
                catch (Exception ex)
                {
                    ConsoleAnimation($"Error Deleting doorstop_config.ini: {ex.InnerException}");
                }
                try
                {
                    File.Delete($"{lethalCompanyPath}\\winhttp.dll");
                }
                catch (Exception ex)
                {
                    ConsoleAnimation($"Error Deleting winhttp.dll: {ex.InnerException}");
                }
                SetColor(ConsoleColor.Green); ;
                ConsoleAnimation("All mod files removed!");
                Thread.Sleep(2500);
            }
            else
            {
                SetColor(ConsoleColor.DarkRed);
                ConsoleAnimation("No mods installed.");
                ConsoleAnimation("Press any key to continue...");
                Console.ReadLine();
                AppHandler();
            }
            Console.Clear();
            AppHandler();
        }
        public static void ReloadUpdater()
        {
            // Rahhh spaghetti code
            SetColor(ConsoleColor.Magenta);
            ConsoleAnimation("Checking for updates...");
            if (File.Exists("LethalUpdater.exe"))
            {
                File.Delete("LethalUpdater.exe");
            }
            using (var client = server)
            {
                // Check Versions
                var verions = server.DownloadString(GenerateSecureDownload("https://cdn.astroswrld.club/secure/Versions/version", 10));
                var updVersion = verions.Split('|')[0];
                var lethalVersion = verions.Split('|')[1];

                ConsoleAnimation($"Grabbed Versions! [Step 1]");

                // Download Updater
                client.DownloadFileAsync(new Uri(GenerateSecureDownload("https://cdn.astroswrld.club/secure/Files/LethalUpdater.exe", 30)), $"LethalUpdater.exe");
                while (client.IsBusy)
                {
                    Thread.Sleep(100);
                }
                ConsoleAnimation($"Updater Reloaded! New Version {updVersion} [Step 2]");

                // Check Astro Version
                if (lethalVersion != currentVersion)
                {
                    ConsoleAnimation($"New Version Available! {lethalVersion} We recommend updating astro to the latest version.");
                    ConsoleAnimation("Press any key to continue...");
                    Console.ReadLine();
                }
                else
                {
                    ConsoleAnimation("No Updates Available. [Step 3]");
                    Thread.Sleep(300);
                }

            }
        }

        // Utility Functions
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
        public static void GetInstalledModNames()
        {
            Console.Clear();
            try
            {
                foreach (var files in Directory.GetFiles(pluginsPath))
                { 
                    if (files.Contains(".dll"))
                    {
                        var tmp = files.Split(new string[] { "plugins" }, StringSplitOptions.None);
                        var name = tmp[1].Split(new string[] { ".dll" }, StringSplitOptions.None);
                        ConsoleAnimation(name[0]);
                    }   
                }
            }
            catch (Exception ex)
            {
                ConsoleAnimation($"Error: {ex.Message}");
            }
        }
        public static void ForceAppUpdate()
        {
            Process.Start("LethalUpdater.exe");
            Environment.Exit(0);
        }
        public static void CheckCustomPath()
        {
            if (File.Exists("cpath.txt"))
            {
                lethalCompanyPath = File.ReadAllText("cpath.txt").ToString();
                Console.Clear();
                ConsoleAnimation(lethalCompanyPath);
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
        public static void CheckLethalCompany()
        {
            Process[] lc = Process.GetProcessesByName("Lethal Company");
            if (lc.Length != 0)
            {
                SetColor(ConsoleColor.DarkRed);
                Console.Clear();
                ConsoleAnimation("Lethal Company is currently running. Please close the game before continuing.\n");
                ConsoleAnimation("Press any key to continue...");
                Console.ReadLine();
                CheckLethalCompany();
            }
        }
        public static bool CheckForExistingMods()
        {
            if (Directory.Exists(bepInExPath) || Directory.Exists(lethalCompanyPath + "\\MLLoader") || File.Exists($"{lethalCompanyPath}\\winhttp.dll") || File.Exists($"{lethalCompanyPath}\\doorstop_config.ini"))
            {
                return true;
            }
            return false;
        }
        public static bool CheckForExistingShaders()
        {
            if (Directory.Exists(lethalCompanyPath + "\\reshade-shaders") || File.Exists(lethalCompanyPath + "\\dxgi.dll") || File.Exists(lethalCompanyPath + "\\ReShade.ini") || File.Exists(lethalCompanyPath + "\\ReShade.log") || File.Exists(lethalCompanyPath + "\\ReShadePreset.ini"))
            {
                return true;
            }
            return false;
        }
        public static void OpenLethalCompanyFolder()
        {
            Process.Start(lethalCompanyPath);
        }
        public static void LaunchSteamGame(int game)
        {
            try
            {
                Process.Start("steam://rungameid/" + game);
            }
            catch (Exception ex)
            {
                SetColor(ConsoleColor.DarkRed);
                ConsoleAnimation($"Error Starting Steam game: {ex.Message}");
            }
        }
        public static void CloseSteamGame(int appId)
        {
            string steamUrl = $"steam://nav/games/details/{appId}";

            try
            {
                // Open the Steam game's details page
                Process.Start(steamUrl);

                // Wait for a moment to ensure the Steam client has enough time to open the game's details
                System.Threading.Thread.Sleep(1000);

                // Find the process associated with the game by its name
                Process[] processes = Process.GetProcessesByName("Lethal Company");
                foreach (Process process in processes)
                {
                    // Close the process
                    process.CloseMainWindow();
                    process.WaitForExit();

                    // I could use Process.Kill but then data would not save properly.
                }
            }
            catch (Exception ex)
            {
                SetColor(ConsoleColor.DarkRed);
                ConsoleAnimation($"Error closing Steam game: {ex.Message}");
            }
        }
        public static string ShowFolderDialog()
        {
            string selectedFolder = null;

            using (var dialog = new FolderBrowserDialog())
            {
                // Set the initial directory (optional)
                dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                // Show the FolderBrowserDialog
                DialogResult result = dialog.ShowDialog();

                // Check if the user clicked OK
                if (result == DialogResult.OK)
                {
                    // Get the selected folder
                    selectedFolder = dialog.SelectedPath;
                }
            }

            return selectedFolder;
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

        // Misc Functions
        public static string GenerateSecureDownload(string url, int expires)
        {
            url = "https://api.astroswrld.club/api/v1/request-token?url=" + url + "&expires=" + expires + "&pureRes=true";

            var response = server.DownloadString(url);
            var resp = response.Split('"');
            return resp[1];
        }
        public static void StopResizing()
        {
            // Thanks CoPilot for Generating This
            const int MF_BYCOMMAND = 0x00000000;
            const int SC_SIZE = 0xF000;
            IntPtr handle = GetConsoleWindow();
            IntPtr sysMenu = GetSystemMenu(handle, false);

            if (handle != IntPtr.Zero)
            {
                DeleteMenu(sysMenu, SC_SIZE, MF_BYCOMMAND);
            }
        }
        public static void GenerateMenu()
        {
            SetColor(ConsoleColor.DarkMagenta);
            CenterText("          :::        ::::::::   :::::::::::   :::::::::       :::::::: \r");
            CenterText("       :+: :+:     :+:    :+:      :+:       :+:    :+:     :+:    :+: \r");
            CenterText("     +:+   +:+    +:+             +:+       +:+    +:+     +:+    +:+  \r");
            CenterText("   +#++:++#++:   +#++:++#++      +#+       +#++:++#:      +#+    +:+   \r");
            CenterText("  +#+     +#+          +#+      +#+       +#+    +#+     +#+    +#+    \r");
            CenterText(" #+#     #+#   #+#    #+#      #+#       #+#    #+#     #+#    #+#     \r");
            CenterText("###     ###    ########       ###       ###    ###      ########       \n");
            SetColor(ConsoleColor.DarkBlue);
            ConsoleAnimation($"Welcome, {Environment.UserName}! (STEAM: {currentSteamId})");
            ConsoleAnimation("Please type a numbered option below, then press enter.");
            SetColor(ConsoleColor.Magenta);
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════╗");
            GenerateOption(new AstroOption()
            {
                option = "View Installed Mods",
                identity = "0",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true,
            });
            GenerateOption(new AstroOption()
            {
                option = "Install Mods.",
                identity = "1",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
            {
                option = "Remove Mods.",
                identity = "2",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
            {
                option = "Extra Mods",
                identity = "3",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true
            });
            SetColor(ConsoleColor.Magenta);
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.WriteLine("╔════════════════════════════════════════════╗");
            GenerateOption(new AstroOption()
            {
                option = "Start / Stop Lethal Company.",
                identity = "4",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
            {
                option = "Open Lethal Company Folder.",
                identity = "5",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true
            });
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.WriteLine("╔════════════════════════════════════════════╗");
            GenerateOption(new AstroOption()
            {
                option = "Force Update Astro Boyz.",
                identity = "6",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
            {
                option = "Remove Custom Path.",
                identity = "7",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true
            });
            Console.WriteLine("╚════════════════════════════════════════════╝\n\n");
            GenerateOption(new AstroOption()
            {
                option = "Option : ",
                identity = "?",
                color = ConsoleColor.Magenta,
                matchMenu = false,
                newLine = false
            });
        }
        public static void ExtrasMenu()
        {
            Console.Clear();
            SetColor(ConsoleColor.DarkMagenta);
            CenterText("          :::        ::::::::   :::::::::::   :::::::::       :::::::: \r");
            CenterText("       :+: :+:     :+:    :+:      :+:       :+:    :+:     :+:    :+: \r");
            CenterText("     +:+   +:+    +:+             +:+       +:+    +:+     +:+    +:+  \r");
            CenterText("   +#++:++#++:   +#++:++#++      +#+       +#++:++#:      +#+    +:+   \r");
            CenterText("  +#+     +#+          +#+      +#+       +#+    +#+     +#+    +#+    \r");
            CenterText(" #+#     #+#   #+#    #+#      #+#       #+#    #+#     #+#    #+#     \r");
            CenterText("###     ###    ########       ###       ###    ###      ########       \n");
            SetColor(ConsoleColor.Magenta);
            Console.WriteLine();
            Console.WriteLine("╔════════════════════════════════════════════╗");
            GenerateOption(new AstroOption() 
            { 
                option = "Astro Menu", 
                identity = "0", 
                color = ConsoleColor.Magenta, 
                matchMenu = true, 
                newLine = true,
                warning = " (CONSIDERED CHEATS)",
                warningColor = ConsoleColor.Cyan
            });
            GenerateOption(new AstroOption()
            {
                option = "Brutal Company",
                identity = "1",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true,
                warning = " (HOST ONLY)",
                warningColor = ConsoleColor.DarkRed
            });
            GenerateOption(new AstroOption()
            {
                option = "Discord Rich Presence",
                identity = "2",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true,
            });
            GenerateOption(new AstroOption()
            {
                option = "Retro Shading",
                identity = "3",
                color = ConsoleColor.Magenta,
                matchMenu = true,
                newLine = true
            });
            Console.WriteLine("╚════════════════════════════════════════════╝\n\n");
            GenerateOption(new AstroOption()
            {
                option = "Option : ",
                identity = "?",
                color = ConsoleColor.Magenta,
                matchMenu = false,
                newLine = false
            });
            var currOption = Console.ReadLine();
            switch (currOption)
            {
                case "0":
                    ConsoleAnimation("Would you like to enable or disable Astro Menu?");
                    GenerateOption(new AstroOption() { option = "Enable", identity = "0", color = ConsoleColor.DarkGreen, matchMenu = false, newLine = true });
                    GenerateOption(new AstroOption() { option = "Disable", identity = "1", color = ConsoleColor.DarkRed, matchMenu = false, newLine = true });
                    var currOption2 = Console.ReadLine();
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
                            SetColor(ConsoleColor.DarkRed);
                            ConsoleAnimation("Invalid Option.");
                            break;
                    }
                    Thread.Sleep(2000);
                    break;
                case "1":
                    ConsoleAnimation("Would you like to enable or disable Brutal Company?");
                    GenerateOption(new AstroOption() { option = "Enable", identity = "0", color = ConsoleColor.DarkGreen, matchMenu = false, newLine = true });
                    GenerateOption(new AstroOption() { option = "Disable", identity = "1", color = ConsoleColor.DarkRed, matchMenu = false, newLine = true });
                    var currOption3 = Console.ReadLine();
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
                            SetColor(ConsoleColor.DarkRed);
                            ConsoleAnimation("Invalid Option.");
                            break;
                    }
                    Thread.Sleep(2000);
                    break;
                case "2":
                    ConsoleAnimation("Would you like to enable or disable Discord Rich Presence?");
                    GenerateOption(new AstroOption()
                    {
                        option = "Enable",
                        identity = "0",
                        color = ConsoleColor.DarkGreen,
                        matchMenu = false,
                        newLine = true
                    });
                    GenerateOption(new AstroOption()
                    {
                        option = "Disable",
                        identity = "1",
                        color = ConsoleColor.DarkRed,
                        matchMenu = false,
                        newLine = true
                    });
                    var currOption4 = Console.ReadLine();
                    switch (currOption4)
                    {
                        case "0":
                            SetColor(ConsoleColor.DarkRed);
                            ConsoleAnimation("Error: This feature is currently disabled.");
                            break;
                        case "1":
                            SetColor(ConsoleColor.DarkRed);
                            ConsoleAnimation("Error: This feature is currently disabled.");
                            break;
                        default:
                            SetColor(ConsoleColor.DarkRed);
                            ConsoleAnimation("Invalid Option.");
                            break;
                    }
                    Thread.Sleep(2000);
                    break;
                case "3":
                    ConsoleAnimation("Would you like to Install or Remove Retro Shading?");
                    GenerateOption(new AstroOption()
                    {
                        option = "Install",
                        identity = "0",
                        color = ConsoleColor.DarkGreen,
                        matchMenu = false,
                        newLine = true
                    });
                    GenerateOption(new AstroOption()
                    {
                        option = "Remove",
                        identity = "1",
                        color = ConsoleColor.DarkRed,
                        matchMenu = false,
                        newLine = true
                    });
                    var currOption5 = Console.ReadLine();
                    switch (currOption5)
                    {
                        case "0":
                            RetroShading(true);
                            break;
                        case "1":
                            RetroShading(false);
                            break;
                        default:
                            SetColor(ConsoleColor.DarkRed);
                            ConsoleAnimation("Invalid Option.");
                            break;
                    }
                    Thread.Sleep(2000);
                    break;
                case "9":
                    Console.Clear();
                    AppHandler();
                    break;
                default:
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation("Invalid Option.");
                    Thread.Sleep(2000);
                    break;
            }
        }
        public static void GenerateOption(AstroOption options)
        {
            // lol
            var Option = options.option ?? "";
            var identity = options.identity ?? "";
            var color = options.color ?? ConsoleColor.Gray;
            var matchMenu = options.matchMenu ?? true;
            var newLine = options.newLine ?? true;
            var warning = options.warning ?? "";
            var warningColor = options.warningColor ?? ConsoleColor.Gray;

            // set cursor to console center
            var originalConsoleColor = Console.ForegroundColor;
            SetColor(color);
            if (matchMenu == true)
            {
                Console.Write("║[ ");
            }
            else
            {
                Console.Write(" [ ");
            }   
            SetColor(ConsoleColor.Gray);
            Console.Write(identity);
            SetColor(color);
            Console.Write(" ] ");
            SetColor(ConsoleColor.Gray);
            if (newLine == true)
            {
                if (matchMenu == true)
                {
                    var old = Console.CursorLeft;
                    Console.SetCursorPosition(45, Console.CursorTop);
                    SetColor(color);
                    Console.Write("║");
                    SetColor(ConsoleColor.Gray);
                    Console.SetCursorPosition(old, Console.CursorTop);
                }
                Console.Write(Option);
                SetColor(warningColor);
                Console.Write(warning + "\n");
            }
            else 
            {
                if (matchMenu == true)
                {
                    var old = Console.CursorLeft;
                    Console.SetCursorPosition(45, Console.CursorTop);
                    SetColor(color);
                    Console.Write("║");
                    SetColor(ConsoleColor.Gray);
                    Console.SetCursorPosition(old, Console.CursorTop);
                }
                Console.Write(Option);
                SetColor(warningColor);
                Console.Write(warning);
            }
            SetColor(originalConsoleColor);
        }
    }
    public class AstroOption
    {
        public string option { get; set; }
        public string warning { get; set; }
        public string identity { get; set; }
        public ConsoleColor? color { get; set; }
        public ConsoleColor? warningColor { get; set; }
        public bool? matchMenu { get; set; }
        public bool? newLine { get; set; }
    }
    public class ServerObj
    {
        public string url { get; set; }
    }
    public class AstroObj
    {
        public string astroVers { get; set; }
        public string updaterVers { get; set; }
    }
    // Thanks again CoPilot for Generating This
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int blockCount = 35;
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private const string animation = @"|/-\";

        private readonly System.Threading.Timer timer;

        private double currentProgress = 0;
        private string currentText = string.Empty;
        private bool disposed = false;
        private int animationIndex = 0;

        public ProgressBar()
        {
            timer = new System.Threading.Timer(TimerHandler);
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(double value)
        {
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref currentProgress, value);
        }

        private void TimerHandler(object state)
        {
            lock (timer)
            {
                if (disposed) return;

                int progressBlockCount = (int)(currentProgress * blockCount);
                int percent = (int)(currentProgress * 100);
                string text = string.Format("[{0}{1}] {2,3}% {3}",
                    new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount),
                    percent,
                    animation[animationIndex++ % animation.Length]);
                UpdateText(text);

                ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            int commonPrefixLength = 0;
            int commonLength = Math.Min(currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', currentText.Length - commonPrefixLength);
            outputBuilder.Append(text.Substring(commonPrefixLength));
            int overlapCount = currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            Console.Write(outputBuilder);
            currentText = text;
        }

        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (timer)
            {
                disposed = true;
                UpdateText(string.Empty);
            }
        }

    }
}