using AstroClient.Systems;
using NAudio.SoundFont;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AstroClient.Objects;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AstroClient.Client
{
    internal class MenuManager
    {
        public static string brutalState;
        public static string astroState;
        public static string richState;
        public static string retroState;
        public static string vrState;
        public static bool musicStarted = false;
        public static void Start()
        {
            DiscordManager.UpdatePresence("idle", "Main Menu");
            Console.Clear();
            GenerateMenu();
            MenuInput();
            Start();
        }
        public static void MenuInput()
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
                                SteamManager.LaunchSteamGame(1966720);
                                DiscordManager.UpdatePresence("cog", "Starting Game");
                                Task.Delay(1000).Wait();
                                Console.Clear();
                                break;
                            case "1":
                                ConsoleSystem.SetColor(Color.Cyan);
                                ConsoleSystem.AnimatedText("Closing...");
                                SteamManager.CloseSteamGame(1966720);
                                DiscordManager.UpdatePresence("idle", "In Menus");
                                Task.Delay(1000).Wait();
                                Console.Clear();
                                break;
                            default:
                                ConsoleSystem.SetColor(Color.DarkRed);
                                ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                                break;
                        }
                        Task.Delay(1000).Wait();
                        Console.Clear();
                        break;
                    case "5":
                        ConsoleSystem.SetColor(Color.Cyan);
                        ConsoleSystem.AnimatedText("Opening...");
                        DiscordManager.UpdatePresence("cog", "Opening Folder");
                        FileSystem.OpenFolderOrFile(Program.lethalCompanyPath);
                        Task.Delay(1000).Wait();
                        Console.Clear();
                        break;
                    case "6":
                        ConsoleSystem.SetColor(Color.Cyan);
                        ConsoleSystem.AnimatedText("Backing Up Mods...");
                        ModManager.BackupMods();
                        break;
                    case "7":
                        ConsoleSystem.SetColor(Color.Cyan);
                        DiscordManager.UpdatePresence("cog", "Updating App");
                        UpdateManager.DownloadAppUpdate();
                        break;
                    case "8":
                        DiscordManager.UpdatePresence("cog", "Viewing Changelog");
                        ChangeLog();
                        break;
                    case "9":
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
                ConsoleSystem.SetColor(Color.BlueViolet);
                // Center the version information
                string versionInfo = UpdateManager.Version.ToString().PadLeft(Console.WindowWidth / 2 + UpdateManager.Version.ToString().Length / 2);
                Console.WriteLine(versionInfo); ConsoleSystem.SetColor(Color.BlueViolet);
                string msg = "Please type the number of the option you would like.\n";
                Console.WriteLine();
                ConsoleSystem.AnimatedText(msg.PadLeft(Console.WindowWidth / 2 + msg.Length / 2));
                ConsoleSystem.SetColor(Color.DeepPink);
                ConsoleSystem.CenterText($"┌──── Mods ──────────────────────────────┐");
                List<MenuOption> options = new List<MenuOption>
                {
                    new MenuOption() { option = "Install Mods", identity = "1", color = Color.BlueViolet, matchMenu = true, newLine = true },
                    new MenuOption() { option = "Remove Mods", identity = "2", color = Color.BlueViolet, matchMenu = true, newLine = true },
                    new MenuOption() { option = "Extra Mods", identity = "3", color = Color.BlueViolet, matchMenu = true, newLine = true }
                };

                foreach (MenuOption option in options)
                {
                    ConsoleSystem.GenerateOption(option);
                }
                ConsoleSystem.CenterText("└────────────────────────────────────────┘");
                ConsoleSystem.CenterText("┌──── Util ──────────────────────────────┐");
                List<MenuOption> utilOptions = new List<MenuOption>
                {
                    new MenuOption() { option = "Start / Stop Lethal Company", identity = "4", color = Color.BlueViolet, matchMenu = true, newLine = true },
                    new MenuOption() { option = "Open Lethal Company Folder", identity = "5", color = Color.BlueViolet, matchMenu = true, newLine = true },
                    new MenuOption() { option = "Backup Old Mods", identity = "6", color = Color.BlueViolet, matchMenu = true, newLine = true }
                };

                foreach (MenuOption option in utilOptions)
                {
                    ConsoleSystem.GenerateOption(option);
                }

                ConsoleSystem.CenterText("└────────────────────────────────────────┘");
                ConsoleSystem.CenterText("┌──── App ───────────────────────────────┐");
                List<MenuOption> appOptions = new List<MenuOption>
                {
                    new MenuOption() { option = "Force Update Astro Boyz", identity = "7", color = Color.BlueViolet, matchMenu = true, newLine = true },
                    new MenuOption() { option = "View Change Log", identity = "8", color = Color.BlueViolet, matchMenu = true, newLine = true },
                    new MenuOption() { option = "Settings", identity = "9", color = Color.BlueViolet, matchMenu = true, newLine = true }
                };

                foreach (MenuOption option in appOptions)
                {
                    ConsoleSystem.GenerateOption(option);
                }

                ConsoleSystem.CenterText("└────────────────────────────────────────┘");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An error occurred in GenerateMenu: {ex}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
            }
            finally
            {
                LogSystem.Log("GenerateMenu Function Ended");
            }
        }
        public static void ExtrasMenu()
        {
            LogSystem.Log("ExtrasMenu Function Started");
            DiscordManager.UpdatePresence("cog", "Viewing Extras Menu");
            try
            {
                GetExtrasStates();
                ConsoleSystem.AppArt();
                ConsoleSystem.SetColor(Color.DeepPink);
                Console.WriteLine();
                ConsoleSystem.CenterText("┌────────────────────────────────────────┐");
                List<MenuOption> menuOptions = new List<MenuOption>
                {
                    new MenuOption { option = "Game Controls", identity = "0", color = Color.BlueViolet, matchMenu = true, newLine = true },
                    new MenuOption { option = "Brutal Company", identity = "1", color = Color.BlueViolet, matchMenu = true, newLine = true, warning = brutalState },
                    new MenuOption { option = "Retro Shading", identity = "2", color = Color.BlueViolet, matchMenu = true, newLine = true, warning = retroState },
                    new MenuOption { option = "Saves Manager", identity = "3", color = Color.BlueViolet, matchMenu = true, newLine = true },
                    new MenuOption { option = "VR Mode", identity = "4", color = Color.BlueViolet, matchMenu = true, newLine = true, warning = vrState },
                    new MenuOption { option = "Back", identity = "5", color = Color.BlueViolet, matchMenu = true, newLine = true }
                };

                foreach (var menuOption in menuOptions)
                {
                    ConsoleSystem.GenerateOption(menuOption);
                }

                ConsoleSystem.CenterText("└────────────────────────────────────────┘\n");

                var currOption = Console.ReadLine();
                Console.WriteLine("");
                ProcessExtrasOption(currOption);
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An error occurred in ExtrasMenu: {ex}");
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
                        GameControlsMenu();
                        break;
                    case "1":
                        ToggleFeature("Brutal Company", ModManager.BrutalCompany);
                        break;
                    case "2":
                        ToggleFeature("Retro Shading", ModManager.RetroShading, "Install", "Remove");
                        break;
                    case "3":
                        SavesMenu();
                        break;
                    case "4":
                        ToggleFeature("VR Mode", ModManager.VRMode);
                        break;
                    case "5":
                        LogSystem.Log("Returning to Main Menu");
                        break;
                    default:
                        LogSystem.Log("Invalid Option Selected in Extras Menu");
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText("Invalid Option.");
                        Task.Delay(2000).Wait();
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
                    Task.Delay(2000).Wait();
                }
            }
        }
        public static void SavesMenu()
        {
            LogSystem.Log("SavesMenu Function Started");
            ConsoleSystem.AppArt();
            ConsoleSystem.SetColor(Color.DeepPink);
            Console.WriteLine();
            var save1 = SaveManager.GetStats("Save 1");
            var save2 = SaveManager.GetStats("Save 2");
            var save3 = SaveManager.GetStats("Save 3");
            Colorful.Console.WriteLine(ConsoleSystem.CenterTextV2("Pick a save file. To pick one, just type the number."));
            Colorful.Console.WriteLine(ConsoleSystem.CenterTextV2("Type the number 4 to return to main menu."));
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.AppendLine(ConsoleSystem.CenterTextV2("┌────────────────────────────────────────────────────────────────────┐"));
            outputBuilder.AppendLine(ConsoleSystem.CenterTextV2($"│ Save 1 │ {$"Credits: {save1[0]}",-14} │ Deadline: {int.Parse(save1[1].ToString()[0].ToString()),-10} │ Quota: {save1[2],-10} │"));
            outputBuilder.AppendLine(ConsoleSystem.CenterTextV2($"│ Save 2 │ {$"Credits: {save2[0]}",-14} │ Deadline: {int.Parse(save2[1].ToString()[0].ToString()),-10} │ Quota: {save2[2],-10} │"));
            outputBuilder.AppendLine(ConsoleSystem.CenterTextV2($"│ Save 3 │ {$"Credits: {save3[0]}",-14} │ Deadline: {int.Parse(save3[1].ToString()[0].ToString()),-10} │ Quota: {save3[2],-10} │"));
            outputBuilder.AppendLine(ConsoleSystem.CenterTextV2("└────────────────────────────────────────────────────────────────────┘"));
            string output = outputBuilder.ToString();
            Colorful.Console.WriteWithGradient(output, Color.BlueViolet, Color.Wheat, 10);
            var currOption = Console.ReadLine();
            Console.WriteLine();
            LogSystem.Log($"User selected option {currOption}");
            switch (currOption)
            {
                case "1":
                    SavePrompt();
                    break;
                case "2":
                    SavePrompt();
                    break;
                case "3":
                    SavePrompt();
                    break;
                case "4":
                    break;
                default:
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Invalid Option.");
                    break;
            }
            void SavePrompt()
            {
                ConsoleSystem.AnimatedText("Would you like to modify or export this save?");
                ConsoleSystem.GenerateOption(new MenuOption() { option = "Modify", identity = "0", matchMenu = false, newLine = true });
                ConsoleSystem.GenerateOption(new MenuOption() { option = "Export", identity = "1", matchMenu = false, newLine = true });
                ConsoleSystem.GenerateOption(new MenuOption() { option = "Import", identity = "2", matchMenu = false, newLine = true });
                ConsoleSystem.GenerateOption(new MenuOption() { option = "Delete", identity = "3", matchMenu = false, newLine = true });
                ConsoleSystem.GenerateOption(new MenuOption() { option = "Back", identity = "4", matchMenu = false, newLine = true });
                var currOption2 = Console.ReadLine();
                Console.WriteLine();
                LogSystem.Log($"User selected option {currOption2}");
                switch (currOption2)
                {
                    case "0":
                        GenerateSaveMenu($"Save {currOption}");
                        break;
                    case "1":
                        SaveManager.ExportSave($"Save {currOption}");
                        ConsoleSystem.AnimatedText("Save Exported! You can find it in your downloads. To load this save in game,");
                        ConsoleSystem.AnimatedText("Simply replace the save file in the saves folder with this one.");
                        ConsoleSystem.AnimatedText("Alternatively, you can import this save using the import option.");
                        ConsoleSystem.AnimatedText("Your saves are located here: " + SaveManager.GameSavePath);
                        ConsoleSystem.AnimatedText("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case "2":
                        ConsoleSystem.AnimatedText("Would you like to import a preset or a custom save?");
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Preset", identity = "0", matchMenu = false, newLine = true });
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Custom", identity = "1", matchMenu = false, newLine = true });
                        var currOption3 = Console.ReadLine();
                        Console.WriteLine();
                        LogSystem.Log($"User selected option {currOption3}");
                        switch (currOption3)
                        {
                            case "0":
                                ConsoleSystem.AnimatedText("Enter the name of the preset you wish to import.");
                                string availablePresets = string.Join(", ", SaveManager.presetLibrary.presets.Select(p => p.Name));
                                ConsoleSystem.AnimatedText($"Current Available Preset(s): {availablePresets}");

                                var currOption4 = Console.ReadLine();
                                Console.WriteLine();
                                LogSystem.Log($"User entered {currOption4}");
                                if (currOption4 != null)
                                {
                                    SaveManager.presetLibrary.ImportPreset($"Save {currOption}", currOption4);
                                    ConsoleSystem.AnimatedText("Save Imported! You can find it in your saves folder.");
                                    ConsoleSystem.AnimatedText("Your saves are located here: " + SaveManager.GameSavePath);
                                    ConsoleSystem.AnimatedText("Press any key to continue...");
                                    Console.ReadKey();
                                }
                                SavesMenu();
                                break;
                            case "1":
                                ConsoleSystem.AnimatedText("Type the path to the save you wish to import, or press enter to browse.");
                                ConsoleSystem.AnimatedText("Example: C:\\Users\\User\\Downloads\\SaveToImport");
                                var currOption5 = Console.ReadLine();
                                Console.WriteLine();
                                LogSystem.Log($"User entered {currOption5}");
                                if (currOption5 != null)
                                {
                                    SaveManager.ImportSave($"{currOption}", currOption5);
                                    ConsoleSystem.AnimatedText("Save Imported! You can find it in your saves folder.");
                                    ConsoleSystem.AnimatedText("Your saves are located here: " + SaveManager.GameSavePath);
                                    ConsoleSystem.AnimatedText("Press any key to continue...");
                                    Console.ReadKey();
                                }
                                break;
                            default:
                                ConsoleSystem.SetColor(Color.DarkRed);
                                ConsoleSystem.AnimatedText("Invalid Option.");
                                Task.Delay(2000).Wait();
                                break;
                        }
                        break;
                    case "3":
                        ConsoleSystem.AnimatedText("Are you sure you want to delete this save?");
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Yes", identity = "0", matchMenu = false, newLine = true });
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "No", identity = "1", matchMenu = false, newLine = true });
                        var currOption6 = Console.ReadLine();
                        Console.WriteLine();
                        LogSystem.Log($"User selected option {currOption6}");
                        switch (currOption6)
                        {
                            case "0":
                                SaveManager.DeleteSave($"Save {currOption}");
                                ConsoleSystem.AnimatedText("Save Deleted!");
                                ConsoleSystem.AnimatedText("Press any key to continue...");
                                Console.ReadKey();
                                break;
                            case "1":
                                break;
                            default:
                                ConsoleSystem.SetColor(Color.DarkRed);
                                ConsoleSystem.AnimatedText("Invalid Option.");
                                Task.Delay(2000).Wait();
                                break;
                        }
                        break;
                    case "4":
                        break;
                    default:
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText("Invalid Option.");
                        Task.Delay(2000).Wait();
                        SavesMenu();
                        break;
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
                LogSystem.Log("Console window and buffer size set");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error setting console window/buffer size: {ex}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
                return;
            }
            ConsoleSystem.SetColor(Color.BlueViolet);
            Console.WriteLine();
            ConsoleSystem.CenterText($"Menu Music: {ConfigSystem.loadedConfig.backgroundMusic}");
            ConsoleSystem.CenterText($"Auto Update: {ConfigSystem.loadedConfig.autoUpdateAstro}");
            ConsoleSystem.CenterText($"Custom Path: {ConfigSystem.loadedConfig.customPath ?? "Steam"}");
            ConsoleSystem.SetColor(Color.DeepPink);
            Console.WriteLine();
            ConsoleSystem.CenterText("┌────────────────────────────────────────┐");

            List<MenuOption> menuOptions = new List<MenuOption>
            {
                new MenuOption { option = "Auto Update", identity = "0", color = Color.BlueViolet, matchMenu = true, newLine = true },
                new MenuOption { option = "Allow Menu Music", identity = "1", color = Color.BlueViolet, matchMenu = true, newLine = true },
                new MenuOption { option = "Favorite Song", identity = "2", color = Color.BlueViolet, matchMenu = true, newLine = true },
                new MenuOption { option = "Music Volume", identity = "3", color = Color.BlueViolet, matchMenu = true, newLine = true },
                new MenuOption { option = "Text Animations", identity = "4", color = Color.BlueViolet, matchMenu = true, newLine = true },
                new MenuOption { option = "Open Logs", identity = "5", color = Color.BlueViolet, matchMenu = true, newLine = true },
                new MenuOption { option = "Back", identity = "6", color = Color.BlueViolet, matchMenu = true, newLine = true }
            };

            foreach (var menuOption in menuOptions)
            {
                ConsoleSystem.GenerateOption(menuOption);
            }
            ConsoleSystem.CenterText("└────────────────────────────────────────┘\n");

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
                        Task.Delay(1000).Wait();
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
                        Task.Delay(1000).Wait();
                        break;
                    case "2":
                        MusicMenu();
                        break;
                    case "3":
                        ConsoleSystem.AnimatedText("Enter the new volume you wish to have. MAX = 1");
                        var currOption4 = Console.ReadLine();
                        Console.WriteLine();
                        LogSystem.Log($"User entered {currOption4}");
                        if (currOption4 != null)
                        {
                            var newAmount = Convert.ToSingle(currOption4);
                            if (newAmount > 1)
                            {
                                newAmount = 1;
                            }
                            ConfigSystem.loadedConfig.musicVolume = newAmount;
                            ConfigSystem.loadedConfig.Save();
                            LogSystem.Log($"Volume changed to {newAmount}");
                            return;
                        }
                        break;
                    case "4":
                        ConsoleSystem.AnimatedText("Would you like to enable or disable Text Animations?");
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Enable", identity = "0", matchMenu = false, newLine = true });
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Disable", identity = "1", matchMenu = false, newLine = true });
                        var currOption5 = Console.ReadLine(); Console.WriteLine();
                        switch (currOption5)
                        {
                            case "0":
                                DiscordManager.UpdatePresence("cog", "Enabling Text Animations");
                                ConfigSystem.loadedConfig.animatedText = true;
                                ConfigSystem.loadedConfig.Save();
                                ConsoleSystem.AnimatedText("Text Animations Enabled!");
                                break;
                            case "1":
                                DiscordManager.UpdatePresence("cog", "Disabling Text Animations");
                                ConfigSystem.loadedConfig.animatedText = false;
                                ConfigSystem.loadedConfig.Save();
                                ConsoleSystem.AnimatedText("Text Animations Disabled!");
                                break;
                            default:
                                ConsoleSystem.SetColor(Color.DarkRed);
                                ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                                break;
                        }
                        Task.Delay(1000).Wait();
                        break;
                    case "5":
                        DiscordManager.UpdatePresence("cog", "Opening Log Folder");
                        Process.Start("explorer.exe", Directory.GetCurrentDirectory() + "\\Logs");
                        Task.Delay(1000).Wait();
                        break;
                    case "6":
                        break;
                    default:
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                        Task.Delay(1000).Wait();
                        SettingsMenu();
                        break;
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An error occurred in SettingsMenu: {ex}");
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
                LogSystem.Log("Console window and buffer size set");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error setting console window/buffer size: {ex}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
                return;
            }

            ConsoleSystem.SetColor(Color.DeepPink);
            ConsoleSystem.CenterText("┌────────────────────────────────────────┐");

            var i = 0;
            foreach (var song in MusicManager.library.GetAllSongs())
            {
                i++;
                ConsoleSystem.GenerateOption(new MenuOption()
                {
                    option = TruncateName(song.Name, 32),
                    identity = song.Number.ToString(),
                    color = Color.BlueViolet,
                    matchMenu = true,
                    newLine = true
                });
                LogSystem.Log($"Menu option generated for {song.Name}");
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
            ConsoleSystem.CenterText("└────────────────────────────────────────┘");

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

                if (int.TryParse(currOption, out int selectedNumber) && MusicManager.library.IsValidSongNumber(selectedNumber))
                {
                    var selectedSong = MusicManager.library.GetSongByNumber(selectedNumber);
                    DiscordManager.UpdatePresence("cog", $"Setting Priority Song");
                    ConsoleSystem.AnimatedText($"Playing {selectedSong.Name}...");
                    ConfigSystem.loadedConfig.prioritySong = selectedSong.Name;
                    ConfigSystem.loadedConfig.Save();
                    MusicManager.prioritySong = true;
                    LogSystem.Log($"Playing song: {selectedSong.Name}");
                }
                else
                {
                    LogSystem.Log("Invalid option selected.");
                    ConsoleSystem.AnimatedText("Invalid option selected.");
                    Task.Delay(2000).Wait();
                    MusicMenu();
                    return;
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An error occurred: {ex}");
                ConsoleSystem.AnimatedText("An error occurred. Please check the log file for details.");
            }
            finally
            {
                LogSystem.Log("MusicMenu Function Ended");
                Task.Delay(1000).Wait();
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
        public static void GameControlsMenu()
        {
            if (!ModManager.CheckLethalCompany())
            {
                LogSystem.Log("Lethal Company not running");
                ConsoleSystem.AnimatedText("Lethal Company must be running for this.");
                Task.Delay(2000).Wait();
                return;
            }
            LogSystem.Log("GameControlsMenu Function Started");
            DiscordManager.UpdatePresence("cog", "Viewing Game Controls Menu");
            try
            {
                ConsoleSystem.AppArt();
                ConsoleSystem.SetColor(Color.DeepPink);
                Console.WriteLine();
                Process[] lc = Process.GetProcessesByName("Lethal Company");
                if (lc.Length < 1)
                {
                    ConsoleSystem.AnimatedText("Lethal Company must be running for this.");
                    Task.Delay(2000).Wait();
                    return;
                }
                ConsoleSystem.CenterText("┌────────────────────────────────────────┐");
                List<MenuOption> menuOptions = new List<MenuOption>
                {
                    new MenuOption { option = "God Mode", identity = "0", color = Color.BlueViolet, matchMenu = true, newLine = true, warning = $"({(ModManager.GodMode ? "Enabled" : "Disabled")})" },
                    new MenuOption { option = "Infinite Stamina", identity = "1", color = Color.BlueViolet, matchMenu = true, newLine = true, warning = $"({(ModManager.InfiniteStamina ? "Enabled" : "Disabled")})" },
                    new MenuOption { option = "Back", identity = "2", color = Color.BlueViolet, matchMenu = true, newLine = true }
                };
                foreach (var menuOption in menuOptions)
                {
                    ConsoleSystem.GenerateOption(menuOption);
                }
                ConsoleSystem.CenterText("└────────────────────────────────────────┘\n");
                var currOption = Console.ReadLine();
                Console.WriteLine();
                switch (currOption)
                {
                    case "0":
                        ConsoleSystem.AnimatedText("Would you like to enable or disable God Mode?");
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Enable", identity = "0", matchMenu = false, newLine = true });
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Disable", identity = "1", matchMenu = false, newLine = true });
                        var currOption2 = Console.ReadLine(); Console.WriteLine();
                        switch (currOption2)
                        {
                            case "0":
                                DiscordManager.UpdatePresence("cog", "Enabling God Mode");
                                ModManager.GodMode = true;
                                SendUpdate();
                                ConsoleSystem.AnimatedText("God Mode Enabled!");
                                break;
                            case "1":
                                DiscordManager.UpdatePresence("cog", "Disabling God Mode");
                                ModManager.GodMode = false;
                                SendUpdate();
                                ConsoleSystem.AnimatedText("God Mode Disabled!");
                                break;
                            default:
                                ConsoleSystem.SetColor(Color.DarkRed);
                                ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                                Task.Delay(2000).Wait();
                                break;
                        }
                        GameControlsMenu();
                        break;
                    case "1":
                        ConsoleSystem.AnimatedText("Would you like to enable or disable Infinite Stamina?");
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Enable", identity = "0", matchMenu = false, newLine = true });
                        ConsoleSystem.GenerateOption(new MenuOption() { option = "Disable", identity = "1", matchMenu = false, newLine = true });
                        var currOption3 = Console.ReadLine(); Console.WriteLine();
                        switch (currOption3)
                        {
                            case "0":
                                DiscordManager.UpdatePresence("cog", "Enabling Infinite Stamina");
                                ModManager.InfiniteStamina = true;
                                SendUpdate();
                                ConsoleSystem.AnimatedText("Infinite Stamina Enabled!");
                                break;
                            case "1":
                                DiscordManager.UpdatePresence("cog", "Disabling Infinite Stamina");
                                ModManager.InfiniteStamina = false;
                                SendUpdate();
                                ConsoleSystem.AnimatedText("Infinite Stamina Disabled!");
                                break;
                            default:
                                ConsoleSystem.SetColor(Color.DarkRed);
                                ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                                Task.Delay(2000).Wait();
                                break;
                        }
                        GameControlsMenu();
                        break;
                    case "2":
                        break;
                    default:
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText("Invalid Option. Please try again.");
                        GameControlsMenu();
                        break;
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in ControlsMenu {ex}");
                return;
            }
            void SendUpdate()
            {
                var newData = new WebSocketObjects()
                {
                    GodMode = ModManager.GodMode,
                    InfiniteStamina = ModManager.InfiniteStamina
                };
                Program.server.SendAsync(JsonConvert.SerializeObject(newData));
            }
        }
        // Extras
        public static void GenerateSaveMenu(string Save)
        {
            LogSystem.Log($"GenerateSaveMenu Function Started: {Save}");
            ConsoleSystem.AppArt();
            ConsoleSystem.SetColor(Color.DeepPink);
            Console.WriteLine();
            ConsoleSystem.CenterText("Select the data you wish to modify");
            ConsoleSystem.CenterText($"┌──── {Save} ────────────────────────────┐");
            List<MenuOption> menuOptions = new List<MenuOption>
            {
                new MenuOption { option = "Credits / Currency", identity = "0", color = Color.BlueViolet, matchMenu = true, newLine = true },
                new MenuOption { option = "Time Left Until Quota", identity = "1", color = Color.BlueViolet, matchMenu = true, newLine = true },
                new MenuOption { option = "Quota Amount", identity = "2", color = Color.BlueViolet, matchMenu = true, newLine = true },
                new MenuOption { option = "Back", identity = "3", color = Color.BlueViolet, matchMenu = true, newLine = true }
            };

            foreach (var menuOption in menuOptions)
            {
                ConsoleSystem.GenerateOption(menuOption);
            }

            string updatedText = "└────────────────────────────────────────┘\n\n";
            ConsoleSystem.CenterText(updatedText);

            var currOption = Console.ReadLine();
            Console.WriteLine();
            switch (currOption)
            {
                case "0":
                    ConsoleSystem.AnimatedText("Enter the new amount of credits you wish to have.");
                    var currOption2 = Console.ReadLine();
                    Console.WriteLine();
                    LogSystem.Log($"User entered {currOption2}");
                    if (currOption2 != null)
                    {
                        var newAmount = Convert.ToInt32(currOption2);
                        var newData = new GameData()
                        {
                            CoinCount = newAmount
                        };
                        SaveManager.ModifyGameData(Save, newData);
                    }
                    break;
                case "1":
                    ConsoleSystem.AnimatedText("Enter the new amount of time you wish to have.");
                    ConsoleSystem.AnimatedText("Example: 1 = 1 Day. Please no more than 3.");
                    var currOption3 = Console.ReadLine();
                    Console.WriteLine();
                    LogSystem.Log($"User entered {currOption3}");
                    if (currOption3 != null)
                    {
                        var newAmount = Convert.ToInt32(currOption3);
                        if (newAmount > 3 || newAmount < 1)
                        {
                            ConsoleSystem.SetColor(Color.DarkRed);
                            ConsoleSystem.AnimatedText("Operation Failed, above game limits.");
                            LogSystem.Log("User entered a value defying game limits.");
                            break;
                        }
                        newAmount = (newAmount == 1) ? 1274 : (newAmount == 2) ? 2255 : (newAmount == 3) ? 3240 : newAmount;

                        var newData = new GameData()
                        {
                            TimeLeft = newAmount
                        };
                        SaveManager.ModifyGameData(Save, newData);
                    }
                    break;
                case "2":
                    ConsoleSystem.AnimatedText("Enter the new amount of quota you wish to have.");
                    var currOption4 = Console.ReadLine();
                    Console.WriteLine();
                    LogSystem.Log($"User entered {currOption4}");
                    if (currOption4 != null)
                    {
                        var newAmount = Convert.ToInt32(currOption4);
                        var newData = new GameData()
                        {
                            QuotaAmount = newAmount
                        };
                        SaveManager.ModifyGameData(Save, newData);
                    }
                    break;
                case "3":
                    break;
                default:
                    ConsoleSystem.SetColor(Color.DarkRed);
                    ConsoleSystem.AnimatedText("Invalid Option.");
                    break;
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
            if (CheckForVR() == true)
            {
                vrState = "Enabled";
            }
            else
            {
                vrState = "Disabled";
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
        public static bool CheckForVR()
        {
            if (FileSystem.ReadIniValue($"{Program.lethalCompanyPath}\\BepInEx\\config\\io.daxcess.lcvr.cfg", "General", "DisableVR") == "true")
            {
                return false;
            }
            return true;
        }
        public static void ChangeLog()
        {
            ConsoleSystem.holdResize = true;
            Console.SetWindowSize(100, 45);
            Console.SetBufferSize(100, 45);
            Colorful.Console.ReplaceAllColorsWithDefaults();
            Console.Clear();
            ConsoleSystem.AppArt();
            Colorful.Console.WriteWithGradient(ServerManager.currentChangelog, Color.BlueViolet, Color.White, 10);
            Console.WriteLine();
            ConsoleSystem.AnimatedText("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
            ConsoleSystem.holdResize = false;
        }
    }
}
