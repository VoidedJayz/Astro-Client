using DiscordRPC;
using Astro.Classes;
using Microsoft.Win32;
using NAudio.Wave;
using Newtonsoft.Json;
using Objects;
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
using System.Windows;
using System.Windows.Forms;
using ProgressBar = Astro.Classes.ProgressBar;
using LethalModz.Classes;
internal class ConsoleMain
{
    // Added This Here to make life easy
    public static bool rerollSong = false;
    public static bool prioritySong = false;
    public static Dictionary<string, string> music = new Dictionary<string, string>()
    {
         { "Drag Me Down by One Direction", "MenuMusic\\Drag_Me_Down.mp3" },
         { "FLAUNT by SUPXR", "MenuMusic\\Flaunt.mp3" },
         { "RAPTURE by INTERWORLD", "MenuMusic\\RAPTURE.mp3" },
         { "I am... All Of Me by Crush 40", "MenuMusic\\I_Am_All_Of_Me.mp3" },
         { "Painkiller by Three Days Grace", "MenuMusic\\Painkiller.mp3" },
         { "Messages From The Stars by The Rah Band", "MenuMusic\\messages_from_the_stars.mp3" },
         { "Candle Queen by Ghost and Pals", "MenuMusic\\Candle_Queen.mp3" },
         { "Just An Attraction by TryHardNinja", "MenuMusic\\Just_An_Attraction.mp3" }
    };
    public static Dictionary<string, string> musicIndex = new Dictionary<string, string>()
    {
         { "1", "Drag Me Down by One Direction" },
         { "2", "FLAUNT by SUPXR" },
         { "3", "RAPTURE by INTERWORLD" },
         { "4", "I am... All Of Me by Crush 40" },
         { "5", "Painkiller by Three Days Grace" },
         { "6", "Messages From The Stars by The Rah Band" },
         { "7", "Candle Queen by Ghost and Pals" },
         { "8", "Just An Attraction by TryHardNinja" }
    };

    [STAThread]
    private static async Task Main(string[] args)
    {
        AstroLogs.Start();
        // Priority Functions
        await AstroWindowManager.Enable();
        await AstroServer.SetupData();
        await AstroUtils.CheckDependencies();
        await AstroConfig.GetConfig();
        await AstroUtils.CheckForUpdates();
        AstroUtils.CheckSteamInstallData();
        AstroUtils.RefreshPath();

        // Make Sure Path Exists
        if (!AstroFileSystem.DirectoryExists(AstroUtils.lethalCompanyPath))
        {
            AstroUtils.GetFolderPath();
        }

        // Begin Application Loop
        SillyMusicHandler();
        DiscordHandler();
        AppHandler();
    }

    public static async Task SillyMusicHandler()
    {
        var currentConfig = AstroUtils.currentConfig;
        string songKey = null;
        string filePath = null;
        bool musicStopped = false;

        CheckPrioritySong();
        AstroLogs.Log("Checking for priority song.");

        try
        {
            var audioFile = new AudioFileReader(filePath);
            AstroLogs.Log($"Starting playback for file: {filePath}");

            using (var outputDevice = new WaveOutEvent())
            {
                outputDevice.Init(audioFile);
                outputDevice.PlaybackStopped += (sender, e) =>
                {
                    AstroLogs.Log("Playback stopped.");

                    if (rerollSong)
                    {
                        AstroLogs.Log("Rerolling song due to reroll request.");
                        return;
                    }
                    if (prioritySong)
                    {
                        AstroLogs.Log("Maintaining priority song.");
                        return;
                    }
                    if (currentConfig.menuMusic)
                    {
                        outputDevice.Stop();
                        audioFile.Dispose();
                        ReRoll();
                        audioFile = new AudioFileReader(filePath);
                        outputDevice.Init(audioFile);
                        outputDevice.Play();
                        AstroLogs.Log("Restarting playback with new song after reroll.");
                    }
                };
                outputDevice.Volume = 0.6f;
                AstroLogs.Log("Audio playback volume set.");

                while (true)
                {
                    if (rerollSong)
                    {
                        outputDevice.Stop();
                        audioFile.Dispose();
                        ReRoll();
                        audioFile = new AudioFileReader(filePath);
                        outputDevice.Init(audioFile);
                        rerollSong = false;
                        AstroLogs.Log("Song rerolled and playback restarted.");
                    }
                    if (prioritySong)
                    {
                        outputDevice.Stop();
                        audioFile.Dispose();
                        CheckPrioritySong();
                        audioFile = new AudioFileReader(filePath);
                        outputDevice.Init(audioFile);
                        prioritySong = false;
                        AstroLogs.Log("Priority song updated and playback restarted.");
                    }
                    else if (!musicStopped)
                    {
                        await Task.Delay(1000);
                        if (currentConfig.menuMusic == true)
                        {
                            outputDevice.Play();
                            string formattedCurrentPosition = String.Format("{0:mm\\:ss}", audioFile.CurrentTime);
                            string formattedEndPosition = String.Format("{0:mm\\:ss}", audioFile.TotalTime);
                            Console.Title = $"ASTRO BOYZ! | {AstroUtils.currentVersion} | ♫ {songKey} ♪ ({formattedCurrentPosition}/{formattedEndPosition})";
                        }
                        else
                        {
                            outputDevice.Pause();
                            Console.Title = $"ASTRO BOYZ! | {AstroUtils.currentVersion} | ";
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            AstroLogs.Log($"Error in SillyMusicHandler: {ex.Message}");
            Console.WriteLine($"Error: {ex.Message}");
        }

        void ReRoll()
        {
            Random random = new Random();
            List<string> keys = new List<string>(music.Keys);
            int randomIndex;
            string newSongKey;

            do
            {
                randomIndex = random.Next(keys.Count);
                newSongKey = keys[randomIndex];
            } while (newSongKey == songKey);

            songKey = newSongKey;
            filePath = music[songKey];
            AstroLogs.Log($"Song rerolled to: {songKey}");
        }

        void CheckPrioritySong()
        {
            if (currentConfig.priotitySong != null)
            {
                if (music.ContainsKey(currentConfig.priotitySong))
                {
                    songKey = currentConfig.priotitySong;
                    filePath = music[songKey];
                    AstroLogs.Log($"Priority song found: {songKey}");
                }
                else
                {
                    ReRoll();
                }
            }
            else
            {
                ReRoll();
            }
        }
    }
    public static void DiscordHandler()
    {
        try
        {
            AstroLogs.Log("Initializing Discord RPC client.");
            var rpc = new DiscordRpcClient("1187300601042841631");
            rpc.Initialize();
            AstroLogs.Log("Discord RPC client initialized.");

            var presence = new RichPresence()
            {
                Details = AstroUtils.currentVersion.ToString(),
                State = "Managing Mods..",
                Assets = new Assets()
                {
                    LargeImageKey = "ghost",
                }
            };

            AstroLogs.Log($"Setting Discord presence: Version - {AstroUtils.currentVersion}, State - Managing Mods..");
            rpc.SetPresence(presence);
            AstroLogs.Log("Discord presence set successfully.");
        }
        catch (Exception ex)
        {
            AstroLogs.Log($"Error in DiscordHandler: {ex.Message}");
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
        AstroUtils.GenerateMenu();

        // Massive fucking switch statement
        var currOption = Console.ReadLine(); Console.WriteLine();
        if (currOption != null)
        {
            switch (currOption)
            {
                case "0":
                    AstroUtils.GetInstalledModNames();
                    AstroUtils.ConsoleAnimation("Press any key to continue...");
                    Console.ReadKey();
                    Console.SetWindowSize(100, 30);
                    Console.BufferWidth = Console.WindowWidth;
                    Console.BufferHeight = Console.WindowHeight;
                    break;
                case "1":
                    AstroUtils.SetColor(Color.Green);
                    AstroUtils.ConsoleAnimation("Starting Mod Installer...");
                    InstallModz();
                    break;
                case "2":
                    AstroUtils.SetColor(Color.DarkRed);
                    AstroUtils.ConsoleAnimation("Starting Mod Remover...");
                    RemoveModz();
                    break;
                case "3":
                    AstroUtils.ExtrasMenu();
                    break;
                case "4":
                    AstroUtils.SetColor(Color.Cyan);
                    AstroUtils.ConsoleAnimation("Would you like to Start or Stop Lethal Company?");
                    AstroUtils.GenerateOption(new MenuOption()
                    {
                        option = "Start",
                        identity = "0",
                        color = Color.DarkGreen,
                        matchMenu = false,
                        newLine = true
                    });
                    AstroUtils.GenerateOption(new MenuOption()
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
                            AstroUtils.SetColor(Color.Cyan);
                            AstroUtils.ConsoleAnimation("Starting...");
                            AstroUtils.LaunchSteamGame(1966720);
                            Thread.Sleep(1000);
                            Console.Clear();
                            break;
                        case "1":
                            AstroUtils.SetColor(Color.Cyan);
                            AstroUtils.ConsoleAnimation("Closing...");
                            AstroUtils.CloseSteamGame(1966720);
                            Thread.Sleep(1000);
                            Console.Clear();
                            break;
                        default:
                            AstroUtils.SetColor(Color.DarkRed);
                            AstroUtils.ConsoleAnimation("Invalid Option. Please try again.");
                            break;
                    }
                    Thread.Sleep(1000);
                    Console.Clear();
                    break;
                case "5":
                    AstroUtils.SetColor(Color.Cyan);
                    AstroUtils.ConsoleAnimation("Opening...");
                    AstroUtils.OpenLethalCompanyFolder();
                    Thread.Sleep(1000);
                    Console.Clear();
                    break;
                case "6":
                    AstroUtils.SetColor(Color.Cyan);
                    ForceAppUpdate();
                    break;
                case "7":
                    AstroUtils.ChangeLog();
                    break;
                case "8":
                    AstroUtils.SettingsMenu();
                    break;
                default:
                    AstroUtils.SetColor(Color.DarkRed);
                    AstroUtils.ConsoleAnimation("Invalid Option. Please try again.");
                    break;
            }
        }
        AppHandler();
    }
    public static void InstallModz()
    {
        try
        {
            AstroLogs.Log("Starting mod installation process.");

            AstroUtils.CheckLethalCompany();
            AstroLogs.Log("Checked Lethal Company.");

            if (AstroUtils.CheckForExistingMods() == true)
            {
                AstroUtils.SetColor(Color.DarkRed);
                AstroUtils.ConsoleAnimation("Existing Mods Found. Please remove them before installing our pack.");
                AstroUtils.ConsoleAnimation("You can use option 2 on the main menu to remove the mods.");
                AstroUtils.ConsoleAnimation("Press any key to continue...");
                Console.ReadKey();
                AstroLogs.Log("Existing mods found. Aborting installation.");
                return;
            }

            using (var client = AstroUtils.server)
            {
                // More spaghetti code yayyyyy
                AstroUtils.SetColor(Color.Magenta);
                AstroUtils.ConsoleAnimation("Downloading Zip...");
                AstroLogs.Log("Initiating mod pack download.");

                try
                {
                    client.DownloadProgressChanged += (s, e) =>
                    {
                        Console.Title = $"ASTRO BOYZ! | Downloading... | {e.ProgressPercentage}% ({e.BytesReceived}/{e.TotalBytesToReceive})";
                    };
                    client.DownloadFileAsync(new Uri(AstroUtils.files["modpack"]), $"{AstroUtils.lethalCompanyPath}\\temp_astro.zip");
                    AstroLogs.Log("Mod pack download started.");
                }
                catch (Exception ex)
                {
                    AstroUtils.ConsoleAnimation($"Error Downloading Mods: {ex.Message}");
                    AstroLogs.Log($"Error downloading mods: {ex.Message}");
                    Thread.Sleep(2500);
                    return;
                }

                while (client.IsBusy)
                {
                    Thread.Sleep(500);
                }
                AstroUtils.ConsoleAnimation("Zip Download Complete!");
                AstroUtils.ConsoleAnimation("Opening Zip...");
                AstroUtils.SetColor(Color.Magenta);
                AstroLogs.Log("Mod pack zip download complete. Opening zip.");

                try
                {
                    using (ZipArchive archive = ZipFile.OpenRead($"{AstroUtils.lethalCompanyPath}\\temp_astro.zip"))
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
                    AstroUtils.SetColor(Color.Cyan);
                    AstroUtils.ConsoleAnimation("Extracting Files...");
                    AstroLogs.Log("Extracting files from mod pack zip.");
                    ZipFile.ExtractToDirectory($"{AstroUtils.lethalCompanyPath}\\temp_astro.zip", $"{AstroUtils.lethalCompanyPath}");

                    AstroUtils.SetColor(Color.Green);
                    AstroUtils.ConsoleAnimation("Finished!");
                    AstroFileSystem.DeleteFile($"{AstroUtils.lethalCompanyPath}\\temp_astro.zip");
                    AstroLogs.Log("Mod pack installation complete.");
                    Thread.Sleep(2500);
                }
                catch (Exception ex)
                {
                    AstroUtils.SetColor(Color.DarkRed);
                    AstroUtils.ConsoleAnimation($"Error Extracting Files: {ex.Message}");
                    AstroLogs.Log($"Error extracting files: {ex.Message}");
                    AstroUtils.ConsoleAnimation("Perhaps the file was corrupted?");
                    AstroUtils.ConsoleAnimation("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }
        catch (Exception ex)
        {
            AstroLogs.Log($"Error in InstallModz: {ex.Message}");
        }
        finally
        {
            Console.Clear();
            AstroLogs.Log("Mod installation process finished.");
        }
    }
    public static void RemoveModz()
    {
        try
        {
            AstroLogs.Log("Starting mod removal process.");
            AstroUtils.CheckLethalCompany();

            // Check For Currently Installed Modz
            if (AstroUtils.CheckForExistingMods() == true)
            {
                AstroUtils.SetColor(Color.Cyan);
                AstroLogs.Log("Existing mods found. Beginning removal process.");

                try
                {
                    AstroFileSystem.DeleteDirectory(AstroUtils.bepInExPath);
                    AstroLogs.Log($"Deleted directory: {AstroUtils.bepInExPath}");
                }
                catch (Exception ex)
                {
                    AstroLogs.Log($"Error deleting directory (BepInExPath): {ex.Message}");
                }
                try
                {
                    AstroFileSystem.DeleteFile($"{AstroUtils.lethalCompanyPath}\\doorstop_config.ini");
                    AstroLogs.Log($"Deleted file: {AstroUtils.lethalCompanyPath}\\doorstop_config.ini");
                }
                catch (Exception ex)
                {
                    AstroLogs.Log($"Error deleting file (doorstop_config.ini): {ex.Message}");
                }
                try
                {
                    AstroFileSystem.DeleteFile($"{AstroUtils.lethalCompanyPath}\\winhttp.dll");
                    AstroLogs.Log($"Deleted file: {AstroUtils.lethalCompanyPath}\\winhttp.dll");
                }
                catch (Exception ex)
                {
                    AstroLogs.Log($"Error deleting file (winhttp.dll): {ex.Message}");
                }

                AstroUtils.SetColor(Color.Green);
                AstroUtils.ConsoleAnimation("All mod files removed!");
                AstroLogs.Log("All mod files have been successfully removed.");
                Thread.Sleep(2500);
            }
            else
            {
                AstroUtils.SetColor(Color.DarkRed);
                AstroUtils.ConsoleAnimation("No mods installed.");
                AstroLogs.Log("No mods installed. Nothing to remove.");
                AstroUtils.ConsoleAnimation("Press any key to continue...");
                Console.ReadKey();
            }
        }
        catch (Exception ex)
        {
            AstroLogs.Log($"Error in RemoveModz: {ex.Message}");
        }
        finally
        {
            Console.Clear();
            AstroLogs.Log("Mod removal process finished.");
        }
    }
    public static void ForceAppUpdate()
    {
        try
        {
            AstroLogs.Log("Starting application update process.");

            using (WebClient client = new WebClient())
            {
                try
                {
                    client.DownloadFile(AstroUtils.files["astro"], "update.exe");
                    Console.WriteLine("Update downloaded successfully.");
                    AstroLogs.Log("Update downloaded successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error downloading update, Please report to Astro: {ex.InnerException}");
                    AstroLogs.Log($"Error downloading update: {ex.InnerException}");
                    Thread.Sleep(10000);
                    return;
                }
            }

            // Replace the current executable with the updated one
            string currentExecutable = Assembly.GetExecutingAssembly().Location;
            string updatedExecutable = "update.exe";
            AstroLogs.Log("Preparing to replace current executable with the update.");

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
                AstroLogs.Log("Update command executed, application will be restarted.");
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error applying update: {ex.Message}");
                AstroLogs.Log($"Error applying update: {ex.Message}");
                return;
            }
        }
        catch (Exception ex)
        {
            AstroLogs.Log($"Unexpected error in ForceAppUpdate: {ex.Message}");
        }
    }

}