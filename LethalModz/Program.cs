using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

internal class Program
{
    [DllImport("user32.dll")]
    public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

    [DllImport("user32.dll")]
    private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
    [DllImport("kernel32.dll", ExactSpelling = true)]
    private static extern IntPtr GetConsoleWindow();
    private static void Main(string[] args)
    {
        // Begin Application
        Console.Title = $"ASTRO BOYZ {Utilities.currentVersion}";
        Console.CursorVisible = false;
        Console.SetWindowSize(100, 30);
        Utilities.lethalCompanyPath = Utilities.GetSteamPath() + "\\steamapps\\common\\Lethal Company";
        Utilities.RefreshPath();
        Utilities.StopResizing();
        Utilities.CheckLethalCompany();
        Utilities.CheckCustomPath();
        Utilities.ReloadUpdater();

        // Make Sure Path Exists
        if (Directory.Exists(Utilities.lethalCompanyPath))
        {
            Utilities.SetColor(ConsoleColor.Yellow);
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
            Utilities.ConsoleAnimation("Error: Lethal Company Not Found. Please install Lethal Company from Steam.");
            Utilities.ConsoleAnimation("If you believe this was a mistake, Contact VoidedJayz / Astro Boyz for Support.");
            Utilities.ConsoleAnimation("Alternatively, you can try deleting the 'cpath.exe' file if present.");
            Utilities.ConsoleAnimation("Alternatively (x2), you can enter the install path manually.");
            Utilities.GenerateOption("Exit", "1", ConsoleColor.DarkRed);
            Utilities.GenerateOption("Enter Custom Path", "2", ConsoleColor.DarkRed);
            var currOption = Console.ReadLine();
            switch (currOption)
            {
                case "1":
                    Environment.Exit(0);
                    break;
                case "2":
                    Utilities.ConsoleAnimation("Please type the path to Lethal Company using this exact format, or there could be issues.");
                    Utilities.SetColor(ConsoleColor.Green);
                    Utilities.ConsoleAnimation("Example: C:\\Program Files (x86)\\Steam\\steamapps\\common\\Lethal Company");
                    Utilities.SetColor(ConsoleColor.DarkRed);
                    Utilities.ConsoleAnimation("Press 'Enter' when you are finished entering the path.");
                    Utilities.lethalCompanyPath = Console.ReadLine();
                    Utilities.RefreshPath();
                    Utilities.SetColor(ConsoleColor.Yellow);
                    Utilities.ConsoleAnimation($"Custom Path: {Utilities.lethalCompanyPath}");
                    File.WriteAllText("cpath.txt", Utilities.lethalCompanyPath);
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
        public static string currentVersion = "1.1.9";
        public static string lethalCompanyPath = null;
        public static string currentSteamId = null;
        public static string bepInExPath = $"{lethalCompanyPath}\\BepInEx";
        public static string pluginsPath = $"{bepInExPath}\\plugins";
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
        public static void AppHandler()
        {
            Console.Clear();
            try
            {
                // Console Title Displays Lethal Company Location
                Console.Title = $"ASTRO BOYZ! | {Utilities.lethalCompanyPath.Split(new string[] { "Files " }, StringSplitOptions.None)[1]} |";
            }
            catch
            {
                Console.Title = $"ASTRO BOYZ! | {Utilities.lethalCompanyPath} |";
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
                        AppHandler();
                        break;
                    case "1":
                        Console.ForegroundColor = ConsoleColor.Green;
                        ConsoleAnimation("Starting Mod Installer...");
                        InstallModz();
                        break;
                    case "2":
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        ConsoleAnimation("Starting Mod Remover...");
                        RemoveModz();
                        break;
                    case "3":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        ConsoleAnimation("Enabling Brutal Company...");
                        BrutalCompany(true);
                        Thread.Sleep(1000);
                        Environment.Exit(0);
                        break;
                    case "4":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        ConsoleAnimation("Disabling Brutal Company...");
                        BrutalCompany(false);
                        Thread.Sleep(1000);
                        Environment.Exit(0);
                        break;
                    case "5":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        ConsoleAnimation("Opening...");
                        Process.Start($"{lethalCompanyPath}\\Lethal Company.exe");
                        Thread.Sleep(1000);
                        Console.Clear();
                        AppHandler();
                        break;
                    case "6":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        ConsoleAnimation("Opening...");
                        OpenLethalCompanyFolder();
                        Thread.Sleep(1000);
                        Console.Clear();
                        AppHandler();
                        break;
                    case "7":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        ConsoleAnimation("Updating...");
                        ForceAppUpdate();
                        Environment.Exit(0);
                        break;
                    case "8":
                        Console.ForegroundColor = ConsoleColor.Yellow;
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
                        AppHandler();
                        break;
                    /*case "9":
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Environment.Exit(0);
                        break;*/
                    default:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        ConsoleAnimation("Invalid Option. Please try again.");
                        AppHandler();
                        break;
                }
            }
        }
        public static void InstallModz()
        {
            // Check For Currently Installed Modz
            if (Directory.Exists(bepInExPath))
            {
                SetColor(ConsoleColor.DarkRed);
                ConsoleAnimation("Found Existing BepInEx folder! Removing...");
                SetColor(ConsoleColor.Yellow);
                try
                {
                    Directory.Delete(bepInExPath, true);
                }
                catch (Exception ex)
                {
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation($"Error: {ex.Message}");
                }
                try
                {
                    File.Delete($"{lethalCompanyPath}\\doorstop_config.ini");
                }
                catch (Exception ex)
                {
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation($"Error: {ex.Message}");
                }
                try
                {
                    File.Delete($"{lethalCompanyPath}\\winhttp.dll");
                }
                catch (Exception ex)
                {
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation($"Error: {ex.Message}");
                }
            }
            else
            {
                ConsoleAnimation("No mods currently installed.");
                ConsoleAnimation("Proceeding.");
            }
            using (var client = server)
            {
                // More spaghetti code yayyyyy
                SetColor(ConsoleColor.Yellow);
                ConsoleAnimation("Loader Started! Downloading Zip...");
                try
                {
                    client.DownloadFileAsync(new Uri("https://eternityhub.xyz/AstroBoyz/Web/Files/CurrentModz.zip"), $"{lethalCompanyPath}\\temp_astro.zip");
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
                using (ZipArchive archive = ZipFile.OpenRead($"{lethalCompanyPath}\\temp_astro.zip"))
                {
                    using (var progress = new ProgressBar())
                    {
                        for (int i = 0; i <= archive.Entries.Count; i++)
                        {
                            progress.Report((double)i / 100);
                            Console.Title = $"ASTRO BOYZ! | Installing Modz... | {i}/{archive.Entries.Count}";
                            Thread.Sleep(20);
                        }
                    }

                }
                Console.Write($"                                                                    \n");
                Console.SetCursorPosition(0, Console.CursorTop);
                SetColor(ConsoleColor.Yellow);
                ConsoleAnimation("Extracting Files...");
                ZipFile.ExtractToDirectory($"{lethalCompanyPath}\\temp_astro.zip", $"{lethalCompanyPath}");
                SetColor(ConsoleColor.Green);
                ConsoleAnimation("Finished!");
                File.Delete($"{lethalCompanyPath}\\temp_astro.zip");
                Thread.Sleep(2500);
            }
            Console.Clear();
            AppHandler();
        }
        public static void RemoveModz()
        {
            // Check For Currently Installed Modz
            if (Directory.Exists(bepInExPath))
            {
                SetColor(ConsoleColor.Yellow);
                ConsoleAnimation("Found BepInEx folder!");

                // Just to make things look cooler
                foreach (var folders in Directory.GetDirectories(bepInExPath))
                {
                    foreach (var files in Directory.GetFiles(folders))
                    {
                        Thread.Sleep(15);
                        SetColor(ConsoleColor.White);
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write($"                                                                    ");
                        Console.SetCursorPosition(0, Console.CursorTop);
                        Console.Write($"Removed: {files.Split(new string[] { "Lethal Company" }, StringSplitOptions.None)[1]}");
                        File.Delete(files);
                    }
                    Directory.Delete(folders, true);
                }
                Console.Write($"                                                                    \n");
                Console.SetCursorPosition(0, Console.CursorTop);
                Directory.Delete(bepInExPath, true);
                File.Delete(lethalCompanyPath + "\\doorstop_config.ini");
                File.Delete(lethalCompanyPath + "\\winhttp.dll");
                SetColor(ConsoleColor.Green); ;
                ConsoleAnimation("All mod files removed!");
                Thread.Sleep(2500);
            }
            else
            {
                ConsoleAnimation("Error: BepInEx folder not found, no mods installed.");
                ConsoleAnimation("Press any key to continue...");
                Console.ReadLine();
                AppHandler();
            }
            Console.Clear();
            AppHandler();
        }
        public static void BrutalCompany(bool state)
        {
            // Rahhh more spaghetti code
            if (!Directory.Exists($"{bepInExPath}\\!! Brutal Company"))
            {
                SetColor(ConsoleColor.DarkRed);
                ConsoleAnimation("Error: Brutal Company not found. Please contact VoidedJayz / Astro Boyz.");
                Thread.Sleep(1500);
                AppHandler();
                return;
            }
            if (state == true)
            {
                try
                {
                    Console.Clear();
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation("WARNING: You should only be enabling Brutal Company if you plan to be host.");
                    SetColor(ConsoleColor.White);
                    ConsoleAnimation("If you are not host, please keep this disabled or it will cause issues.");
                    ConsoleAnimation("Are you sure you want to enable this?\n");
                    GenerateOption("Yes", "1", ConsoleColor.Magenta);
                    GenerateOption("No\n\n", "2", ConsoleColor.Magenta);
                    GenerateOption("Option : ", "?", ConsoleColor.Magenta, false);
                    var currOption = Console.ReadLine();
                    switch (currOption)
                    {
                        case "1":
                            SetColor(ConsoleColor.Green);
                            File.Move($"{bepInExPath}\\!! Brutal Company\\BrutalCompany.dll", $"{pluginsPath}\\BrutalCompany.dll");
                            ConsoleAnimation("Brutal Company Enabled!");
                            Thread.Sleep(750);
                            Console.Clear();
                            AppHandler();
                            break;
                        case "2":
                            SetColor(ConsoleColor.DarkRed);
                            ConsoleAnimation("Brutal Company Disabled!");
                            File.Move($"{pluginsPath}\\BrutalCompany.dll", $"{bepInExPath}\\!! Brutal Company\\BrutalCompany.dll");
                            Thread.Sleep(750);
                            Console.Clear();
                            AppHandler();
                            break;
                        default:
                            SetColor(ConsoleColor.DarkRed);
                            ConsoleAnimation("Invalid Option. Brutal Company Disabled!");
                            File.Move($"{pluginsPath}\\BrutalCompany.dll", $"{bepInExPath}\\!! Brutal Company\\BrutalCompany.dll");
                            Thread.Sleep(750);
                            Console.Clear();
                            AppHandler();
                            break;
                    }
                }
                catch
                {
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation("Brutal Company Is Already Enabled!");
                }
                Thread.Sleep(1500);
                Console.Clear();
                AppHandler();
            }
            else
            {
                try
                {
                    File.Move($"{pluginsPath}\\BrutalCompany.dll", $"{bepInExPath}\\!! Brutal Company\\BrutalCompany.dll");
                    SetColor(ConsoleColor.Green);
                    ConsoleAnimation("Brutal Company Disabled!");
                }
                catch
                {
                    SetColor(ConsoleColor.DarkRed);
                    ConsoleAnimation("Brutal Company Is Already Disabled!");
                }
                Thread.Sleep(1500);
                Console.Clear();
                AppHandler();
            }
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
                var updVersion = client.DownloadString("https://eternityhub.xyz/AstroBoyz/Web/Files/uv");
                var lethalVersion = client.DownloadString("https://eternityhub.xyz/AstroBoyz/Web/Files/av");
                ConsoleAnimation($"Grabbed Versions! [Step 1]");

                // Download Updater
                client.DownloadFileAsync(new Uri("https://eternityhub.xyz/AstroBoyz/Web/Files/LethalUpdater.exe"), $"LethalUpdater.exe");
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
        public static void OpenLethalCompanyFolder()
        {
            Process.Start(lethalCompanyPath);
        }

        // Misc Functions
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
            ConsoleAnimation("Please select an option below.");
            SetColor(ConsoleColor.Magenta);
            Console.WriteLine();
            Console.WriteLine(">-----------------------------------<");
            GenerateOption("List Mods", "0", ConsoleColor.Magenta);
            GenerateOption("Install Modz.", "1", ConsoleColor.Magenta);
            GenerateOption("Remove Modz.", "2", ConsoleColor.Magenta);
            GenerateOption("Enable/Disable Brutal Company.", "3/4", ConsoleColor.Magenta);
            Console.WriteLine(">-----------------------------------<");
            GenerateOption("Start Lethal Company.", "5", ConsoleColor.Magenta);
            GenerateOption("Open Lethal Company Folder.", "6", ConsoleColor.Magenta);
            Console.WriteLine(">-----------------------------------<");
            GenerateOption("(FORCE) Update ASTRO BOYZ.", "7", ConsoleColor.Magenta);
            GenerateOption("Remove Custom Path.", "8", ConsoleColor.Magenta);
            Console.WriteLine(">-----------------------------------<\n\n");
            GenerateOption("Option : ", "?", ConsoleColor.Magenta, false);
        }
        public static void GenerateOption(string Option, string identity, ConsoleColor color, bool newLine = true)
        {
            // Probably much better ways to do this, but this works ig
            var originalConsoleColor = Console.ForegroundColor;
            SetColor(color);
            Console.Write(" [ ");
            SetColor(ConsoleColor.Gray);
            Console.Write(identity);
            SetColor(color);
            Console.Write(" ] ");
            SetColor(ConsoleColor.Gray);
            if (newLine == true)
            {
                Console.Write(Option + "\n");
            }
            else { Console.Write(Option);}
            SetColor(originalConsoleColor);
        }
    }

    // Thanks again CoPilot for Generating This
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int blockCount = 10;
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private const string animation = @"|/-\";

        private readonly Timer timer;

        private double currentProgress = 0;
        private string currentText = string.Empty;
        private bool disposed = false;
        private int animationIndex = 0;

        public ProgressBar()
        {
            timer = new Timer(TimerHandler);
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