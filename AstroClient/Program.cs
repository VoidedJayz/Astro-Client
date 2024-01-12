using AstroClient.Client;
using AstroClient.Systems;
using static AstroClient.Objects;
using System.Drawing;
using System.Diagnostics;
using Newtonsoft.Json;

namespace AstroClient
{
    class Program
    {
        public static string? lethalCompanyPath;
        public static string? bepInExPath;
        public static string? pluginsPath;
        public static bool DebuggerMode = Debugger.IsAttached;
        public static GameClient server;

        [STAThread]
        private static void Main(string[] args)
        {
            try
            {
                server = new GameClient();
                LogSystem.Start();
                WindowManager.Start();
                ServerManager.Start();
                DownloadSystem.Start();
                DependencyManager.Start();
                ConfigSystem.Start();
                SteamManager.Start();
                DiscordManager.Start();
                ModManager.Start();
                SaveManager.Start();
                UpdateManager.Start();
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Astro Client has requested to exit and cannot continue. ");
                Console.WriteLine("You can check the log file for more information, it will be opened in your browser.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Process.Start(new ProcessStartInfo
                {
                    FileName = LogSystem.LatestLogPath,
                    UseShellExecute = true
                });
                Environment.Exit(0);
            }
        }


    }
}