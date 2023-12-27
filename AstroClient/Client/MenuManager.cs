using AstroClient.Systems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AstroClient.Objects;

namespace AstroClient.Client
{
    internal class MenuManager
    {
        public static string brutalState;
        public static string astroState;
        public static string richState;
        public static string retroState;
        public static async Task Start()
        {
            DiscordManager.UpdatePresence("idle", "Main Menu");
            Console.Clear();
            GenerateMenu();
            await MenuInput();
            Start();
        }
        public static async Task MenuInput()
        {
            var currOption = Console.ReadLine(); Console.WriteLine();
            if (currOption != null)
            {
                switch (currOption)
                {
                    case "1":
                        ConsoleSystem.SetColor(Color.Green);
                        ConsoleSystem.AnimatedText("Starting Mod Installer...");
                        ModManager.InstallMods();
                        break;
                    case "2":
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText("Starting Mod Remover...");
                        ModManager.UninstallMods();
                        break;
                    case "3":
                        ExtrasMenu();
                        break;
                    case "4":
                        ConsoleSystem.SetColor(Color.Cyan);
                        ConsoleSystem.AnimatedText("Would you like to Start or Stop Lethal Company?");
                        ConsoleSystem.GenerateOption(new MenuOption()
                        {
                            option = "Start",
                            identity = "0",
                            color = Color.DarkGreen,
                            matchMenu = false,
                            newLine = true
                        });
                        ConsoleSystem.GenerateOption(new MenuOption()
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
                                ConsoleSystem.SetColor(Color.Cyan);
                                ConsoleSystem.AnimatedText("Starting...");
                                SteamSystem.LaunchSteamGame(1966720);
                                DiscordManager.UpdatePresence("cog", "Starting Game");
                                Thread.Sleep(1000);
                                Console.Clear();
                                break;
                            case "1":
                                ConsoleSystem.SetColor(Color.Cyan);
                                ConsoleSystem.AnimatedText("Closing...");
                                SteamSystem.CloseSteamGame(1966720);
                                DiscordManager.UpdatePresence("idle", "In Menus");
                                Thread.Sleep(1000);
                                Console.Clear();
                                break;
                            default:
                                ConsoleSystem.SetColor(Color.DarkRed);
                                ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                                break;
                        }
                        Thread.Sleep(1000);
                        Console.Clear();
                        break;
                    case "5":
                        ConsoleSystem.SetColor(Color.Cyan);
                        ConsoleSystem.AnimatedText("Opening...");
                        DiscordManager.UpdatePresence("cog", "Opening Folder");
                        Process.Start("explorer.exe", $@"{Program.lethalCompanyPath}");
                        Thread.Sleep(1000);
                        Console.Clear();
                        break;
                    case "6":
                        ConsoleSystem.SetColor(Color.Cyan);
                        DiscordManager.UpdatePresence("cog", "Updating App");
                        UpdateManager.DownloadAppUpdate();
                        break;
                    case "7":
                        DiscordManager.UpdatePresence("cog", "Viewing Changelog");
                        ChangeLog();
                        break;
                    case "8":
                        SettingsMenu();
                        break;
                    default:
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                        break;
                }
            }
        }
        public static void GenerateMenu()
        {
            LogSystem.Log("GenerateMenu Function Started");

            try
            {
                ConsoleSystem.AppArt();
                Console.SetWindowSize(100, 30);
                Console.SetBufferSize(100, 30);
                ConsoleSystem.SetColor(Color.HotPink);
                Console.WriteLine($"                                                                                 {UpdateManager.Version}");
                ConsoleSystem.SetColor(Color.BlueViolet);
                ConsoleSystem.AnimatedText("\nPlease type the number of the option you would like.\n");
                ConsoleSystem.SetColor(Color.HotPink);
                Console.WriteLine("╔════ Mods ══════════════════════════════════╗");
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = "Install Mods",
                    identity = "1",
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = "Remove Mods",
                    identity = "2",
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = "Extra Mods",
                    identity = "3",
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                Console.WriteLine("╚════════════════════════════════════════════╝");
                Console.WriteLine("╔════ Util ══════════════════════════════════╗");
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = "Start / Stop Lethal Company",
                    identity = "4",
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = "Open Lethal Company Folder",
                    identity = "5",
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                Console.WriteLine("╚════════════════════════════════════════════╝");
                Console.WriteLine("╔════ App ═══════════════════════════════════╗");
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = "Force Update Astro Boyz",
                    identity = "6",
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = "View Change Log",
                    identity = "7",
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = "Settings",
                    identity = "8",
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                Console.WriteLine("╚════════════════════════════════════════════╝\n");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An error occurred in GenerateMenu: {ex.Message}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
            }
            finally
            {
                LogSystem.Log("GenerateMenu Function Ended");
            }
        }
        public static void ChangeLog()
        {
            Colorful.Console.ReplaceAllColorsWithDefaults();
            Console.Clear();
            ConsoleSystem.AppArt();
            ConsoleSystem.AnimatedText(ServerManager.currentChangelog);
            Console.SetWindowSize(100, 30);
            Console.SetBufferSize(100, 30);
            Console.WriteLine();
            ConsoleSystem.AnimatedText("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
        }
        public static void ExtrasMenu()
        {
            LogSystem.Log("ExtrasMenu Function Started");
            DiscordManager.UpdatePresence("cog", "Viewing Extras Menu");
            try
            {
                GetExtrasStates();
                ConsoleSystem.AppArt();
                ConsoleSystem.SetColor(Color.HotPink);
                ConsoleSystem.AnimatedText($"                                                                                 {UpdateManager.Version}");
                Console.WriteLine("\n╔════════════════════════════════════════════╗");

                string[] options = { "Astro Menu", "Brutal Company", "Retro Shading", "Back" };
                string[] warnings = { " (CONSIDERED CHEATS)", " (HOST ONLY)", "", "" };
                for (int i = 0; i < options.Length; i++)
                {
                    ConsoleSystem.GenerateOption(new MenuOption()
                    {
                        option = options[i],
                        identity = i.ToString(),
                        color = Color.BlueViolet,
                        matchMenu = true,
                        newLine = true,
                        warning = warnings[i]
                    });
                    LogSystem.Log($"Menu option generated for {options[i]}");
                }
                Console.WriteLine("╚════════════════════════════════════════════╝\n\n");

                var currOption = Console.ReadLine();
                Console.WriteLine("");
                ProcessExtrasOption(currOption);
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An error occurred in ExtrasMenu: {ex.Message}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
            }
            finally
            {
                LogSystem.Log("ExtrasMenu Function Ended");
            }
            void ProcessExtrasOption(string currOption)
            {
                switch (currOption)
                {
                    case "0":
                        ToggleFeature("Astro Menu", ModManager.AstroMenu);
                        break;
                    case "1":
                        ToggleFeature("Brutal Company", ModManager.BrutalCompany);
                        break;
                    case "2":
                        ToggleFeature("Retro Shading", ModManager.RetroShading, "Install", "Remove");
                        break;
                    case "3":
                        LogSystem.Log("Returning to Main Menu");
                        break;
                    default:
                        LogSystem.Log("Invalid Option Selected in Extras Menu");
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText("Invalid Option.");
                        Thread.Sleep(2000);
                        break;
                }
                void ToggleFeature(string featureName, Action<bool> toggleAction, string enableOption = "Enable", string disableOption = "Disable")
                {
                    ConsoleSystem.AnimatedText($"Would you like to {enableOption.ToLower()} or {disableOption.ToLower()} {featureName}?");
                    ConsoleSystem.GenerateOption(new MenuOption() { option = enableOption, identity = "0", color = Color.DarkGreen, matchMenu = false, newLine = true });
                    ConsoleSystem.GenerateOption(new MenuOption() { option = disableOption, identity = "1", color = Color.DarkRed, matchMenu = false, newLine = true });

                    var userChoice = Console.ReadLine();
                    Console.WriteLine();
                    switch (userChoice)
                    {
                        case "0":
                            DiscordManager.UpdatePresence("cog", $"Enabling {featureName}");
                            toggleAction(true);
                            ConsoleSystem.AnimatedText($"{featureName} {enableOption}d!");
                            break;
                        case "1":
                            DiscordManager.UpdatePresence("cog", $"Disabling {featureName}");
                            toggleAction(false);
                            ConsoleSystem.AnimatedText($"{featureName} {disableOption}d!");
                            break;
                        default:
                            LogSystem.ReportError("Invalid Option Selected");
                            ConsoleSystem.SetColor(Color.DarkRed);
                            ConsoleSystem.AnimatedText("Invalid Option.");
                            break;
                    }
                    Thread.Sleep(2000);
                }
            }
        }
        public static void SettingsMenu()
        {
            LogSystem.Log("SettingsMenu Function Started");
            DiscordManager.UpdatePresence("cog", "Viewing Settings Menu");
            try
            {
                ConsoleSystem.AppArt();
                Console.SetWindowSize(100, 30);
                Console.SetBufferSize(100, 30);
                LogSystem.Log("Console window and buffer size set");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error setting console window/buffer size: {ex.Message}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
                return;
            }

            ConsoleSystem.SetColor(Color.HotPink);
            Console.WriteLine($"                                                                                 {UpdateManager.Version}");
            ConsoleSystem.SetColor(Color.BlueViolet);
            ConsoleSystem.AnimatedText($"Menu Music: {ConfigSystem.loadedConfig.backgroundMusic}");
            ConsoleSystem.AnimatedText($"Auto Update: {ConfigSystem.loadedConfig.autoUpdateAstro}");
            ConsoleSystem.AnimatedText($"Custom Path: {ConfigSystem.loadedConfig.customPath ?? "Steam"}");
            ConsoleSystem.SetColor(Color.HotPink);
            Console.WriteLine("\n╔════════════════════════════════════════════╗");

            string[] options = { "Auto Update", "Allow Menu Music", "Priority Song", "Back" };
            for (int i = 0; i < options.Length; i++)
            {
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = options[i],
                    identity = i.ToString(),
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                LogSystem.Log($"Menu option generated for {options[i]}");
            }

            Console.WriteLine("╚════════════════════════════════════════════╝\n");

            try
            {
                var currOption = Console.ReadLine(); Console.WriteLine();
                switch (currOption)
                {
                    case "1":
                        ConsoleSystem.AnimatedText("Would you like to enable or disable Menu Music?");
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Enable", identity = "0", matchMenu = false, newLine = true });
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Disable", identity = "1", matchMenu = false, newLine = true });
                        var currOption2 = Console.ReadLine(); Console.WriteLine();
                        switch (currOption2)
                        {
                            case "0":
                                DiscordManager.UpdatePresence("cog", "Enabling Menu Music");
                                ConfigSystem.loadedConfig.backgroundMusic = true;
                                ConfigSystem.loadedConfig.Save();
                                ConsoleSystem.AnimatedText("Music Enabled!");
                                break;
                            case "1":
                                DiscordManager.UpdatePresence("cog", "Disabling Menu Music");
                                ConfigSystem.loadedConfig.backgroundMusic = false;
                                ConfigSystem.loadedConfig.Save();
                                ConsoleSystem.AnimatedText("Music Disabled!");
                                break;
                            default:
                                ConsoleSystem.SetColor(Color.DarkRed);
                                ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                                break;
                        }
                        Thread.Sleep(1000);
                        break;
                    case "0":
                        ConsoleSystem.AnimatedText("Would you like to enable or disable Auto Update?");
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Enable", identity = "0", matchMenu = false, newLine = true });
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Disable", identity = "1", matchMenu = false, newLine = true });
                        var currOption3 = Console.ReadLine(); Console.WriteLine();
                        switch (currOption3)
                        {
                            case "0":
                                DiscordManager.UpdatePresence("cog", "Enabling Auto Update");
                                ConfigSystem.loadedConfig.autoUpdateAstro = true;
                                ConfigSystem.loadedConfig.Save();
                                ConsoleSystem.AnimatedText("Auto Update Enabled!");
                                break;
                            case "1":
                                DiscordManager.UpdatePresence("cog", "Disabling Auto Update");
                                ConfigSystem.loadedConfig.autoUpdateAstro = false;
                                ConfigSystem.loadedConfig.Save();
                                ConsoleSystem.AnimatedText("Auto Update Disabled!");
                                break;
                            case "9":
                                break;
                            default:
                                ConsoleSystem.SetColor(Color.DarkRed);
                                ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                                break;
                        }
                        Thread.Sleep(1000);
                        break;
                    case "2":
                        MusicMenu();
                        break;
                    case "3":
                        break;
                    default:
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                        Thread.Sleep(1000);
                        break;
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An error occurred in SettingsMenu: {ex.Message}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
            }
            finally
            {
                LogSystem.Log("SettingsMenu Function Ended");
            }
        }
        public static void MusicMenu()
        {
            LogSystem.Log("MusicMenu Function Started");
            DiscordManager.UpdatePresence("cog", "Viewing Music Menu");
            try
            {
                ConsoleSystem.AppArt();
                ConsoleSystem.SetColor(Color.BlueViolet);
                Console.SetWindowSize(100, 30);
                Console.SetBufferSize(100, 30);
                LogSystem.Log("Console window and buffer size set");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error setting console window/buffer size: {ex.Message}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
                return;
            }
            ConsoleSystem.SetColor(Color.HotPink);
            Console.WriteLine("\n╔════════════════════════════════════════════╗");
            var i = 0;
            foreach (var item in MusicManager.music)
            {
                i++;
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = TruncateName(item.Key, 35),
                    identity = i.ToString(),
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                LogSystem.Log($"Menu option generated for {item.Key}");
            }
            ConsoleSystem.GenerateOption(new MenuOption()
            {
                option = "None/Remove Priority Song",
                identity = "None",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            ConsoleSystem.GenerateOption(new MenuOption()
            {
                option = "Back To Main Menu",
                identity = "Return",
                color = Color.BlueViolet,
                matchMenu = true,
                newLine = true
            });
            Console.WriteLine("╚════════════════════════════════════════════╝\n");

            try
            {
                var currOption = Console.ReadLine();
                if (currOption == "None")
                {
                    DiscordManager.UpdatePresence("cog", "Removing Priority Song");
                    Console.Clear();
                    ConsoleSystem.AnimatedText("Priority song removed.");
                    ConfigSystem.loadedConfig.prioritySong = null;
                    ConfigSystem.loadedConfig.Save();
                    return;
                }
                if (currOption == "Return")
                {
                    return;
                }
                if (!MusicManager.musicIndex.ContainsKey(currOption))
                {
                    LogSystem.Log("Invalid option selected.");
                    ConsoleSystem.AnimatedText("Invalid option selected. Please check the log file for details.");
                    return;
                }

                DiscordManager.UpdatePresence("cog", $"Setting Priority Song");
                var tempValue = MusicManager.musicIndex[currOption];
                ConsoleSystem.AnimatedText($"Playing {tempValue}...");
                ConfigSystem.loadedConfig.prioritySong = tempValue;
                ConfigSystem.loadedConfig.Save();
                MusicManager.prioritySong = true;
                LogSystem.Log($"Playing song: {tempValue}");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An error occurred: {ex.Message}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
            }
            finally
            {
                LogSystem.Log("MusicMenu Function Ended");
                Thread.Sleep(1000);
            }
            string TruncateName(string input, int maxLength)
            {
                if (input.Length > maxLength)
                {
                    return input.Substring(0, maxLength - 3) + "...";
                }
                return input;
            }
        }
        public static void GetExtrasStates()
        {
            if (FileSystem.FileExists($"{Program.pluginsPath}\\AstroMenu.dll"))
            {
                astroState = "Enabled";
            }
            else
            {
                astroState = "Disabled";
            }
            if (FileSystem.FileExists($"{Program.pluginsPath}\\BrutalCompanyPlus.dll"))
            {
                brutalState = "Enabled";
            }
            else
            {
                brutalState = "Disabled";
            }
            if (FileSystem.FileExists($"{Program.pluginsPath}\\AstroRPC.dll"))
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
    }
}
