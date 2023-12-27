using AstroClient.Client;
using AstroClient.Systems;

namespace AstroClient
{
    class Program
    {
        public static string? lethalCompanyPath;
        public static string? bepInExPath;
        public static string? pluginsPath;
        private static async Task Main(string[] args)
        {
            try
            {
                await LogSystem.Start();
                await WindowManager.Start();
                await DependencyManager.CheckDependencies();
                await ConfigSystem.GetConfig();
                await ServerManager.SetupData();
                await UpdateManager.CheckAppUpdates();
                await UpdateManager.CheckModUpdates();
                await SteamSystem.Start();
                await DiscordManager.Start();
                await ModManager.Start();
                Task.Run(MusicManager.Start);
                await MenuManager.Start();
            }
            catch (Exception ex)
            {
                LogSystem.ReportError(ex.ToString());
            }
            finally
            {
                Console.WriteLine("Astro Client has ran into a fatal error and cannot continue. ");
                Console.WriteLine("You can check the log file for more information. ");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                Environment.Exit(0);
            }
        }
    }
}