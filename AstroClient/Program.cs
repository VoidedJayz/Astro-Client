using AstroClient.Client;
using AstroClient.Systems;
using static AstroClient.Objects;
using System.Drawing;

namespace AstroClient
{
    class Program
    {
        public static string? lethalCompanyPath;
        public static string? bepInExPath;
        public static string? pluginsPath;

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                // Main Stuffs
                LogSystem.Start();
                WindowManager.Start();
                DependencyManager.CheckDependencies();
                ConfigSystem.GetConfig();
                ServerManager.SetupData();
                SteamManager.Start();
                DiscordManager.Start();
                ModManager.Start();
                SaveManager.Start();
                new Thread(MusicManager.Start).Start();
                UpdateManager.CheckAppUpdates();
                MenuManager.Start();
            }
            catch (Exception ex)
            {
                // If astro client crashes, log the error
                LogSystem.ReportError($"Fatal Error in Main: {ex}");
            }
            finally
            {
                // Upon crashing, display a message to the user about the crash
                string logDir = Directory.GetCurrentDirectory();
                string logDirectory = $"{logDir}\\Logs";
                ConsoleSystem.SetColor(System.Drawing.Color.DarkRed);
                ConsoleSystem.AnimatedText("Astro Client has ran into a fatal error and cannot continue. ");
                ConsoleSystem.AnimatedText("You can check the log file for more information.");
                ConsoleSystem.AnimatedText($"Log Files are located at {logDirectory}.");
                ConsoleSystem.AnimatedText("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}