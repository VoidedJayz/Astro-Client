using AstroClient.Client;
using AstroClient.Systems;
using static AstroClient.Objects;
using System.Drawing;
using System.Diagnostics;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Windows.Forms;
namespace AstroClient
{
    class Program
    {
        public static string? lethalCompanyPath;
        public static string? bepInExPath;
        public static string? pluginsPath;
        public static bool VisualStudio = Debugger.IsAttached;
        public static bool isAppUpdating = false;
        public static bool isNewInstall = false;
        public static bool isCrashRecovery = false;
        public static GameClient server;

        [STAThread]
        private static void Main(string[] args)
        {
            // Check for arguments
            isAppUpdating = Array.Exists(args, arg => arg.Equals("AppUpdating", StringComparison.OrdinalIgnoreCase));
            isNewInstall = Array.Exists(args, arg => arg.Equals("FreshInstall", StringComparison.OrdinalIgnoreCase));
            isCrashRecovery = Array.Exists(args, arg => arg.Equals("CrashRecovery", StringComparison.OrdinalIgnoreCase));
            // Check if Astro is Recovering from a crash, if so, display a message
            if (isCrashRecovery)
                CrashRecovery();
            // Check if Astro is already running, if so, exit
            if (IsAlreadyRunning())
                AlreadyRunningTask();
            // Check if Astro is already installed, if not, exit
            if (isNewInstall)
                WelcomeMessage();
            // Main Application
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
                new Thread(MusicManager.Start).Start();
                MenuManager.Start();
            }
            catch (Exception ex)
            {
                // If astro client crashes, log the error with additional details
                LogSystem.ReportError("Fatal Error in Main:", ex.Message);

                // Log the stack trace
                LogSystem.ReportError("StackTrace:", ex.StackTrace);

                // Log inner exceptions recursively, if any
                Exception innerException = ex.InnerException;
                int innerExceptionCount = 1;

                while (innerException != null)
                {
                    LogSystem.ReportError($"InnerException {innerExceptionCount}:", innerException.Message);
                    LogSystem.ReportError($"InnerException {innerExceptionCount} StackTrace:", innerException.StackTrace);

                    innerException = innerException.InnerException;
                    innerExceptionCount++;
                }
            }

            finally
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = Process.GetCurrentProcess().MainModule.FileName,
                    UseShellExecute = true,
                    Arguments = "CrashRecovery"
                });
                Environment.Exit(0);
            }
        }


        static bool IsAlreadyRunning()
        {
            if (isAppUpdating)
                Task.Delay(1000).Wait();

            Process currentProcess = Process.GetCurrentProcess();
            var existingProcesses = Process.GetProcessesByName(currentProcess.ProcessName);
            return existingProcesses.Length > 1;
        }
        static void AlreadyRunningTask()
        {
            Process currentProcess = Process.GetCurrentProcess();
            var existingProcesses = Process.GetProcessesByName(currentProcess.ProcessName);
            ConsoleSystem.SetForegroundWindow(existingProcesses[0].MainWindowHandle);
            Environment.Exit(0);
        }
        static void WelcomeMessage()
        {
            // Show a nice, one time welcome message to the user
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Welcome to Astro Client!");
            Console.WriteLine("Astro Client is a mod loader packed full of features for Lethal Company.");
            Console.WriteLine("Astro Client is not affiliated with Lethal Company in any way, shape, or form.");
            Console.WriteLine("Astro Client is open source, and you can view the source code on GitHub.");
            Console.WriteLine("We hope you enjoy Astro Client, happy modding!");
            Console.WriteLine("Press any key to continue.");
            Console.ReadKey();
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
        }
        static void CrashRecovery()
        {
            FileSystem.OpenFolderOrFile(LogSystem.logDirectory);
            MessageBox.Show("Astro Client is recovering from a crash. \nBecause of this, the client has restarted.\nWe've opened the logs directory for you.", "Astro Client", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}