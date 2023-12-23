using DiscordRPC;
using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
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
internal class Astro
{
    private static AstroConfig currentConfig;

    [STAThread]
    private static void Main(string[] args)
    {
        AstroUtils.CheckDependencis();
        currentConfig = AstroConfig.Load();
        Console.Title = $"Astro Boyz {AstroUtils.currentVersion}";
        Console.CursorVisible = false;
        Console.SetWindowSize(100, 30);
        AstroUtils.SillyMusicHandler();
        AstroUtils.CheckForUpdates();
        AstroUtils.CheckSteamInstallData();
        AstroUtils.CheckLethalCompany();
        AstroUtils.RefreshPath();
        AstroUtils.KeepSize();
        AstroUtils.DiscordHandler();

        // Make Sure Path Exists
        if (Directory.Exists(AstroUtils.lethalCompanyPath))
        {
            AstroUtils.SetColor(Color.Cyan);
            try
            {
                Console.Title = $"ASTRO BOYZ! | {AstroUtils.lethalCompanyPath.Split(new string[] { "Files " }, StringSplitOptions.None)[1]} |";
            }
            catch
            {
                Console.Title = $"ASTRO BOYZ! | {AstroUtils.lethalCompanyPath} |";
            }
        }
        else
        {
            AstroUtils.SetColor(Color.DarkRed);
            Console.Title = "ASTRO BOYZ! | Lethal Company Not Found";
            AstroUtils.GetFolderPath();
        }

        // Begin Application Loop
        AstroUtils.AppHandler();
    }
    private class AstroUtils
    {
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
            { "MenuMusic\\Comatose.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Comatose.mp3" },
            { "MenuMusic\\FLAUNT.mp3", "https://cdn.astroswrld.club/Client/Dependencies/FLAUNT.mp3" },
            { "MenuMusic\\Drag_Me_Down.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Drag_Me_Down.mp3" },
            { "MenuMusic\\RAPTURE.mp3", "https://cdn.astroswrld.club/Client/Dependencies/RAPTURE.mp3" }
        };
        public static WebClient server = new WebClient();
        public static string currentVersion = "2.5.0";
        public static string lethalCompanyPath = null;
        public static string currentSteamId = null;
        public static string currentSteamName = null;
        public static string bepInExPath = $"{lethalCompanyPath}\\BepInEx";
        public static string pluginsPath = $"{bepInExPath}\\plugins";
        public static string brutalState;
        public static string astroState;
        public static string richState;
        public static string retroState;

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

        // Application Functions
        public static async Task SillyMusicHandler()
        {
            var options = new List<string>()
            {
                "MenuMusic\\Comatose.mp3",
                "MenuMusic\\FLAUNT.mp3",
                "MenuMusic\\Drag_Me_Down.mp3",
                "MenuMusic\\RAPTURE.mp3"
            };
            Random random = new Random();
            int randomIndex = random.Next(options.Count);
            string filePath = options[randomIndex];
            try
            {
                using (var audioFile = new AudioFileReader(filePath))
                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.PlaybackStopped += (sender, e) =>
                    {
                        if (currentConfig.menuMusic)
                        {
                            audioFile.Position = 0;
                            outputDevice.Play();
                        }
                    };
                    outputDevice.Volume = 0.6f;
                    while (true)
                    {
                        await Task.Delay(1000);
                        if (currentConfig.menuMusic == true)
                        {
                            outputDevice.Play();
                        }
                        else
                        {
                            outputDevice.Pause();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        public static void DiscordHandler()
        {
            var rpc = new DiscordRpcClient("1187300601042841631");
            rpc.Initialize();
            var presence = new RichPresence()
            {
                Details = currentVersion,
                State = "Managing Mods..",
                Assets = new Assets()
                {
                    LargeImageKey = "ghost",
                }
            };

            rpc.SetPresence(presence);
        }
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
            var enabledPath = $"{pluginsPath}\\AstroRPC.dll";
            var disabledPath = $"{bepInExPath}\\core\\AstroRPC.dll";
            // Check if file is missing
            if (!File.Exists(enabledPath) || !File.Exists(disabledPath))
            {
                ConsoleAnimation("AstroRPC.dll is missing.");
                Thread.Sleep(2000);
                return;
            }
            if (state == true) 
            {   
                if (richState == "Enabled")
                {
                    ConsoleAnimation($"Rich Presence is already {richState}.");
                    return;
                }
                else
                {
                    try
                    {
                        File.Move(disabledPath, enabledPath);
                    }
                    catch (Exception ex)
                    {
                        ConsoleAnimation($"Error: {ex.Message}");
                        Thread.Sleep(2000);
                    }
                }
            }
            else
            {
                if (richState == "Disabled")
                {
                    ConsoleAnimation($"Rich Presence is already {richState}.");
                    return;
                }
                else
                {
                    try
                    {
                        File.Move(enabledPath, disabledPath);
                    }
                    catch (Exception ex)
                    {
                        ConsoleAnimation($"Error: {ex.Message}");
                        Thread.Sleep(2000);
                    }
                }
            }
        }
        public static void RetroShading(bool state)
        {
            if (state == true)
            {
                if (CheckForExistingShaders() == true)
                {
                    SetColor(Color.DarkRed);
                    ConsoleAnimation("Existing Shaders Found. Please remove them before installing new shaders.");
                    ConsoleAnimation("Press any key to continue...");
                    Console.ReadKey();
                    AppHandler();
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
                        client.DownloadFileAsync(new Uri(GenerateSecureDownload(files["shader"], 30)), $"{lethalCompanyPath}\\RetroShader.ini");
                        while (client.IsBusy)
                        {
                            Thread.Sleep(500);
                        }
                        client.DownloadFileAsync(new Uri(GenerateSecureDownload(files["instructions"], 30)), $"{lethalCompanyPath}\\intructions.txt");
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
                    if (File.Exists($"{lethalCompanyPath}\\ReShade.ini"))
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
                    File.Delete(lethalCompanyPath + "\\setup.exe");
                    File.Delete(lethalCompanyPath + "\\RetroShader.ini");
                    File.Delete(lethalCompanyPath + "\\intructions.txt");
                    SetColor(Color.Green);
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
                Console.Title = $"Astro Boyz | {AstroUtils.lethalCompanyPath.Split(new string[] { "Files " }, StringSplitOptions.None)[1]} |";
            }
            catch
            {
                Console.Title = $"Astro Boyz | {AstroUtils.lethalCompanyPath} |";
            }
            // Menu Options
            GenerateMenu();

            // Massive fucking switch statement
            var currOption = Console.ReadLine(); Console.WriteLine();
            if (currOption != null)
            {
                switch (currOption)
                {
                    case "0":
                        GetInstalledModNames();
                        ConsoleAnimation("Press any key to continue...");
                        Console.ReadKey();
                        Console.SetWindowSize(100, 30);
                        Console.BufferWidth = Console.WindowWidth;
                        Console.BufferHeight = Console.WindowHeight;
                        break;
                    case "1":
                        SetColor(Color.Green);
                        ConsoleAnimation("Starting Mod Installer...");
                        InstallModz();
                        break;
                    case "2":
                        SetColor(Color.DarkRed);
                        ConsoleAnimation("Starting Mod Remover...");
                        RemoveModz();
                        break;
                    case "3":
                        ExtrasMenu();
                        break;
                    case "4":
                        SetColor(Color.Cyan);
                        ConsoleAnimation("Would you like to Start or Stop Lethal Company?");
                        GenerateOption(new AstroOption()
                        {
                            option = "Start",
                            identity = "0",
                            color = Color.DarkGreen,
                            matchMenu = false,
                            newLine = true
                        });
                        GenerateOption(new AstroOption()
                        {
                            option = "Stop",
                            identity = "1",
                            color = Color.DarkRed,
                            matchMenu = false,
                            newLine = true
                        });
                        var currOption2 = Console.ReadLine(); Console.WriteLine();
                        switch (currOption2)
                        {
                            case "0":
                                SetColor(Color.Cyan);
                                ConsoleAnimation("Starting...");
                                LaunchSteamGame(1966720);
                                Thread.Sleep(1000);
                                Console.Clear();
                                break;
                            case "1":
                                SetColor(Color.Cyan);
                                ConsoleAnimation("Closing...");
                                CloseSteamGame(1966720);
                                Thread.Sleep(1000);
                                Console.Clear();
                                break;
                            default:
                                SetColor(Color.DarkRed);
                                ConsoleAnimation("Invalid Option. Please try again.");
                                break;
                        }
                        Thread.Sleep(1000);
                        Console.Clear();
                        break;
                    case "5":
                        SetColor(Color.Cyan);
                        ConsoleAnimation("Opening...");
                        OpenLethalCompanyFolder();
                        Thread.Sleep(1000);
                        Console.Clear();
                        break;
                    case "6":
                        SetColor(Color.Cyan);
                        ForceAppUpdate();
                        break;
                    case "7":
                        ChangeLog();
                        break;
                    case "8":
                        SettingsMenu();
                        break;
                    default:
                        SetColor(Color.DarkRed);
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
                SetColor(Color.DarkRed);
                ConsoleAnimation("Existing Mods Found. Please remove them before installing our pack.");
                ConsoleAnimation("You can use option 2 on the main menu tor emove the mods.");
                ConsoleAnimation("Press any key to continue...");
                Console.ReadKey();
                return;
            }
            using (var client = server)
            {
                // More spaghetti code yayyyyy
                SetColor(Color.Magenta);
                ConsoleAnimation("Downloading Zip...");
                try
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        Console.Title = $"ASTRO BOYZ! | Downloading... | {e.ProgressPercentage}% ({e.BytesReceived}/{e.TotalBytesToReceive})";
                    };
                    client.DownloadFileAsync(new Uri(GenerateSecureDownload(files["modpack"], 30)), $"{lethalCompanyPath}\\temp_astro.zip");
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
                SetColor(Color.Magenta);
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
                    SetColor(Color.Cyan);
                    ConsoleAnimation("Extracting Files...");
                    ZipFile.ExtractToDirectory($"{lethalCompanyPath}\\temp_astro.zip", $"{lethalCompanyPath}");
                    SetColor(Color.Green);
                    ConsoleAnimation("Finished!");
                    File.Delete($"{lethalCompanyPath}\\temp_astro.zip");
                    Thread.Sleep(2500);
                }
                catch (Exception ex)
                {
                    SetColor(Color.DarkRed);
                    ConsoleAnimation($"Error Extracting Files: {ex.Message}");
                    ConsoleAnimation("Perhaps the file was corrupted?");
                    ConsoleAnimation("Press any key to continue...");
                    Console.ReadKey();
                }
            }
            Console.Clear();
        }
        public static void RemoveModz()
        {
            // Check For Currently Installed Modz
            if (CheckForExistingMods() == true)
            {
                SetColor(Color.Cyan);
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
                SetColor(Color.Green); ;
                ConsoleAnimation("All mod files removed!");
                Thread.Sleep(2500);
            }
            else
            {
                SetColor(Color.DarkRed);
                ConsoleAnimation("No mods installed.");
                ConsoleAnimation("Press any key to continue...");
                Console.ReadKey();
            }
            Console.Clear();
        }
        public static void ForceAppUpdate()
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(files["astro"], "update.exe");
                    Console.WriteLine("Update downloaded successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading update, Please report to Astro: {ex.InnerException}");
                    Thread.Sleep(10000);
                    return;
                }
            }

            // Replace the current executable with the updated one
            string currentExecutable = Assembly.GetExecutingAssembly().Location;
            string updatedExecutable = "update.exe";

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/C timeout /T 1 /nobreak && move /Y \"{updatedExecutable}\" \"{currentExecutable}\" && start \"\" \"{currentExecutable}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false
                };

                Process.Start(psi);
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying update: {ex.Message}");
                return;
            }

            Process.Start(currentExecutable);
            Process.GetCurrentProcess().Kill();
        }

        // Utility Functions
        public static string GetFullUserName()
        {
            WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

            if (windowsIdentity != null)
            {
                return windowsIdentity.Name;
            }

            return null;
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
        public static void GetFolderPath()
        {
            currentConfig.customPath = null;
            currentConfig.Save();
            // Main Prompt
            ConsoleAnimation("Example: C:\\Program Files (x86)\\Steam\\steamapps\\common\\Lethal Company");
            ConsoleAnimation("Please type a folder path or you can just press 'Enter' to select a folder.");
            string folderPath = Console.ReadLine(); Console.WriteLine();
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                using (var folderBrowserDialog = new FolderBrowserDialog())
                {
                    DialogResult result = folderBrowserDialog.ShowDialog();

                    if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                    {
                        folderPath = folderBrowserDialog.SelectedPath;
                    }
                }
            }
            else if (folderPath.Contains('"'))
            {
                folderPath = folderPath.Split('"')[1];
            }

            try
            {
                ConsoleAnimation(folderPath);
                
            }
            catch { }
            ConsoleAnimation("Path Saved!");

            // Set Other Values to make sure it works
            lethalCompanyPath = folderPath;
            currentConfig.customPath = folderPath;
            currentConfig.Save();
            RefreshPath();
        }
        public static void KeepSize()
        {
            Console.SetWindowSize(100, 30);
            Console.BufferWidth = Console.WindowWidth;
            Console.BufferHeight = Console.WindowHeight;
        }
        public static void SillyMusicMenuOption()
        {
            ConsoleAnimation("Would you like to Play or Pause?");
            GenerateOption(new AstroOption()
            {
                option = "Play",
                identity = "0",
                color = Color.DarkGreen,
                matchMenu = false,
                newLine = true
            });
            GenerateOption(new AstroOption()
            {
                option = "Pause",
                identity = "1",
                color = Color.DarkRed,
                matchMenu = false,
                newLine = true
            });
            var currOption3 = Console.ReadLine(); Console.WriteLine();
            switch (currOption3)
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
        }
        public static void GetExtrasStates() 
        {             
            if (File.Exists($"{pluginsPath}\\AstroMenu.dll"))
            {
                astroState = "Enabled";
            }
            else
            {
                astroState = "Disabled";
            }
            if (File.Exists($"{pluginsPath}\\BrutalCompanyPlus.dll"))
            {
                brutalState = "Enabled";
            }
            else
            {
                brutalState = "Disabled";
            }
            if (File.Exists($"{lethalCompanyPath}\\MLLoader\\Mods\\LethalCompanyPresence.dll"))
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
            if (Directory.Exists(pluginsPath))
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
                Process.Start("steam://rungameid/" + game);
            }
            catch (Exception ex)
            {
                SetColor(Color.DarkRed);
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
                SetColor(Color.DarkRed);
                ConsoleAnimation($"Error closing Steam game: {ex.Message}");
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
        public static void CheckDependencis()
        {
            if (!Directory.Exists("MenuMusic"))
            {
                Directory.CreateDirectory("MenuMusic");
            }
            var dependencyMissing = false;
            using (WebClient client = server)
            {
                foreach (var dependency in dependencies)
                {
                    if (!File.Exists(dependency.Key))
                    {
                        dependencyMissing = true;
                        Console.WriteLine($"Downloading Missing Dependency [{dependency.Key}]...");
                        try
                        {
                            client.DownloadFile(dependency.Value, dependency.Key);
                        }
                        catch (Exception ex)
                        {
                            SetColor(Color.DarkRed);
                            Console.WriteLine($"Error downloading dependency, Please report to Astro: {ex.InnerException}");
                            Thread.Sleep(10000);
                            Environment.Exit(0);
                        }
                    }
                }
            }
            if (dependencyMissing == true)
            {
                Console.WriteLine("All dependencies downloaded successfully.");
                Thread.Sleep(2000);
                Process.Start(Process.GetCurrentProcess().MainModule.FileName);
                Process.GetCurrentProcess().Kill();
            }
        }
        public static void CheckCurrentStates()
        {
            if (File.Exists($"{pluginsPath}\\AstroMenu.dll"))
            {
                astroState = "Enabled";
            }
            else
            {
                astroState = "Disabled";
            }
            if (File.Exists($"{pluginsPath}\\BrutalCompanyPlus.dll"))
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
            Colorful.Console.Clear();
            string steamAppsFolder = GetSteamPath() + "\\steamapps";

            string[] appManifestFiles = Directory.GetFiles(steamAppsFolder, "appmanifest_*.acf");

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
                }

                if (appName == "Lethal Company")
                {
                    lethalCompanyPath = fullPath;
                }
            }
            Thread.Sleep(500);
            string ExtractValue(string line)
            {
                var match = Regex.Match(line, "\"[^\"]+\"\\s+\"([^\"]+)\"");
                return match.Success ? match.Groups[1].Value : "";
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
        public static void CheckForUpdates()
        {
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
            ConsoleAnimation("Checking for updates...");
            var client = new WebClient();
            var response = client.DownloadString("https://astroswrld.club/Client/version");
            var version = new Version(response.Split('|')[1]);
            var current = new Version(currentVersion);
            ConsoleAnimation($"Latest Version: {version}");
            if (current < version)
            {
                if (currentConfig.autoUpdate == true)
                {
                    ConsoleAnimation("Updating..");
                    Thread.Sleep(1000);
                    ForceAppUpdate();
                }
                else
                {
                    ConsoleAnimation("Update Available! Would you like to update now?");
                    GenerateOption(new AstroOption()
                    {
                        option = "Yes",
                        identity = "0",
                        matchMenu = false,
                        newLine = true
                    });
                    GenerateOption(new AstroOption()
                    {
                        option = "No",
                        identity = "1",
                        matchMenu = false,
                        newLine = true
                    });
                    var currOption = Console.ReadLine(); Console.WriteLine();
                    switch (currOption)
                    {
                        case "0":
                            ConsoleAnimation("Updating..");
                            Thread.Sleep(1000);
                            ForceAppUpdate();
                            break;
                        case "1":
                            ConsoleAnimation("Update Skipped.");
                            Thread.Sleep(1000);
                            break;
                        default:
                            SetColor(Color.DarkRed);
                            ConsoleAnimation("Invalid Option. Please try again.");
                            break;
                    }
                }
            }
            else if (current > version)
            {
                ConsoleAnimation("Time Traveler. You have a newer version.");
                Thread.Sleep(2000);
            }
            else
            {
                ConsoleAnimation("No Updates Available!");
                Thread.Sleep(2000);
            }
            client.Dispose();
            Colorful.Console.ReplaceAllColorsWithDefaults();
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
            var DetectedFiles = new List<string>()
            {
                $"{lethalCompanyPath}\\dxgi.dll",
                $"{lethalCompanyPath}\\ReShade.ini",
                $"{lethalCompanyPath}\\ReShade.log",
                $"{lethalCompanyPath}\\ReShadePreset.ini",
                $"{lethalCompanyPath}\\RetroShader.ini",
                $"{lethalCompanyPath}\\setup.exe",
            };
            if (Directory.Exists(lethalCompanyPath + "\\reshade-shaders"))
            {
                return true;
            }
            if (DetectedFiles.Any(File.Exists))
            {
                return true;
            }
            return false;
        }

        // Misc Functions
        public static string GenerateSecureDownload(string url, int expires)
        {
            url = "https://api.astroswrld.club/api/v1/request-token?url=" + url + "&expires=" + expires + "&pureRes=true";

            var response = server.DownloadString(url);
            var resp = response.Split('"');
            return resp[1];
        }
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
            Colorful.Console.WriteWithGradient($@"
{currentVersion} Changelog:
New Features:
    - Group Labels
    - Auto Update Toggle Saves
    - Music Toggle Saves
    - Added Settings Menu
    - Added Discord RPC
    - Added Administrator Request

Bug Fixes:
    - Fixed Music not playing on first launch
    - ReadKey causing issues with menu
", Color.BlueViolet, Color.Purple, 10);
            Console.SetWindowSize(100, 30);
            Console.SetBufferSize(100, 30);
            Console.WriteLine();
            ConsoleAnimation("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
        public static void GenerateMenu()
        {
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
            Console.SetWindowSize(100, 30);
            Console.SetBufferSize(100, 30);
            SetColor(Color.HotPink);
            Console.WriteLine($"                                                                                 {currentVersion}");
            SetColor(Color.BlueViolet);
            ConsoleAnimation("\nPlease type the number of the option you would like.");
            Console.WriteLine();
            Console.WriteLine("╔════ Mods ══════════════════════════════════╗");
            GenerateOption(new AstroOption()
            {
                option = "View Installed Mods",
                identity = "0",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true,
            });
            GenerateOption(new AstroOption()
            {
                option = "Install Mods",
                identity = "1",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
            {
                option = "Remove Mods",
                identity = "2",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
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
            GenerateOption(new AstroOption()
            {
                option = "Start / Stop Lethal Company",
                identity = "4",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
            {
                option = "Open Lethal Company Folder",
                identity = "5",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            Console.WriteLine("╚════════════════════════════════════════════╝");
            Console.WriteLine("╔════ App ═══════════════════════════════════╗");
            GenerateOption(new AstroOption()
            {
                option = "Force Update Astro Boyz",
                identity = "6",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
            {
                option = "View Change Log",
                identity = "7",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
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
            GenerateOption(new AstroOption() 
            { 
                option = "Astro Menu", 
                identity = "0", 
                color = Color.BlueViolet, 
                matchMenu = true, 
                newLine = true,
                warning = " (CONSIDERED CHEATS)"
            });
            GenerateOption(new AstroOption()
            {
                option = "Brutal Company",
                identity = "1",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true,
                warning = " (HOST ONLY)",
            });
            GenerateOption(new AstroOption()
            {
                option = "Retro Shading",
                identity = "2",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
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
                    GenerateOption(new AstroOption() { option = "Enable", identity = "0", color = Color.DarkGreen, matchMenu = false, newLine = true });
                    GenerateOption(new AstroOption() { option = "Disable", identity = "1", color = Color.DarkRed, matchMenu = false, newLine = true });
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
                    GenerateOption(new AstroOption() { option = "Enable", identity = "0", color = Color.DarkGreen, matchMenu = false, newLine = true });
                    GenerateOption(new AstroOption() { option = "Disable", identity = "1", color = Color.DarkRed, matchMenu = false, newLine = true });
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
                    GenerateOption(new AstroOption()
                    {
                        option = "Install",
                        identity = "0",
                        color = Color.DarkGreen,
                        matchMenu = false,
                        newLine = true
                    });
                    GenerateOption(new AstroOption()
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
                    AppHandler();
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
            GenerateOption(new AstroOption()
            {
                option = "Menu Music",
                identity = "0",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
            {
                option = "Auto Update",
                identity = "1",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            GenerateOption(new AstroOption()
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
                case "0":
                    ConsoleAnimation("Would you like to enable or disable Menu Music?");
                    GenerateOption(new AstroOption() { option = "Enable", identity = "0", matchMenu = false, newLine = true });
                    GenerateOption(new AstroOption() { option = "Disable", identity = "1", matchMenu = false, newLine = true });
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
                case "1":
                    ConsoleAnimation("Would you like to enable or disable Auto Update?");
                    GenerateOption(new AstroOption() { option = "Enable", identity = "0", matchMenu = false, newLine = true });
                    GenerateOption(new AstroOption() { option = "Disable", identity = "1",  matchMenu = false, newLine = true });
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
                case "default":
                    SetColor(Color.DarkRed);
                    ConsoleAnimation("Invalid Option. Please try again.");
                    Thread.Sleep(1000);
                    break;
            }

        }
        public static void GenerateOption(AstroOption options)
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
    private class AstroOption
    {
        public string option { get; set; }
        public string warning { get; set; }
        public string identity { get; set; }
        public Color? color { get; set; }
        public Color? warningColor { get; set; }
        public bool? matchMenu { get; set; }
        public bool? newLine { get; set; }
    }
    private class ServerObj
    {
        public string url { get; set; }
    }
    private class AstroObj
    {
        public string astroVers { get; set; }
        public string updaterVers { get; set; }
    }
    private class AstroConfig
    {
        public bool menuMusic { get; set; } = true;
        public bool autoUpdate { get; set; } = true;
        public string customPath { get; set; } = null;

        public void Save()
        {
            File.WriteAllText("settings.json", JsonConvert.SerializeObject(this));
        }
        public static AstroConfig Load()
        {
            if (File.Exists("settings.json"))
            {
                return JsonConvert.DeserializeObject<AstroConfig>(File.ReadAllText("settings.json"));
            }
            return new AstroConfig();
        }
    }
    private class ProgressBar : IDisposable, IProgress<double>
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