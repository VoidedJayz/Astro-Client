using AstroClient.Systems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Client
{
    internal class UpdateManager
    {
        public static Version Version = new Version("3.3.8");
        public static bool IsDeveloper() => Environment.UserName.Equals("voide", StringComparison.OrdinalIgnoreCase);
        public static void Start()
        {
            if (!ConfigSystem.loadedConfig.autoUpdateAstro)
                return;

            LogSystem.Log("Starting update check");
            LogSystem.Log($"Current Astro Version: {Version} | Latest Astro Version: {ServerManager.latestVersion}");
            LogSystem.Log($"Current Modpack Timestamp: {ModManager.Version} | Latest Astro Version: {ServerManager.latestModpackVersion}");
            ConsoleSystem.UpdateArt();
            CheckAstroUpdate();
            CheckModpackUpdate();
            Task.Delay(75).Wait();
        }
        private static void CheckAstroUpdate()
        {
            if (Version < ServerManager.latestVersion)
            {
                ConsoleSystem.SetColor(Color.Cyan);
                ConsoleSystem.AnimatedText("Update Available!");
                LogSystem.Log("Update Available!");
                DownloadAppUpdate();
            }
            else
            {
                ConsoleSystem.SetColor(Color.LimeGreen);
                ConsoleSystem.AnimatedText("No Updates Available for Astro.");
                LogSystem.Log("No Updates Available for Astro.");
                Task.Delay(75).Wait();
            }
        }
        private static void CheckModpackUpdate()
        {
            if (IsDeveloper())
                return;
            if (ModManager.Version < ServerManager.latestModpackVersion)
            {
                ConsoleSystem.SetColor(Color.Cyan);
                ConsoleSystem.AnimatedText("Modpack Available, We highly recommend you update the modpack using Astro.");
                ConsoleSystem.AnimatedText("These updates are essential for peer-to-peer syncing and bug fixes.");
                ConsoleSystem.AnimatedText("Press any key to continue...");
                LogSystem.Log("Modpack Update Available!");
                Console.ReadKey();
            }
            else
            {
                ConsoleSystem.SetColor(Color.LimeGreen);
                ConsoleSystem.AnimatedText("No Updates Available for Modpack.");
                LogSystem.Log("No Updates Available for Modpack.");
                Task.Delay(75).Wait();
            }
        }
        public static void DownloadAppUpdate()
        {
            try
            {
                LogSystem.Log("Starting application update process.");
                DownloadUpdates();
                //ReplaceCurrentExecutable();
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Unexpected error in DownloadAppUpdate: {ex}");
            }
        }
        private static void DownloadUpdates()
        {
            if (!FileSystem.DirectoryExists("Updater"))
            {
                FileSystem.CreateDirectory("Updater");
                LogSystem.Log("Created Updater directory.");
            }
            using (WebClient client = new WebClient())
            {
                try
                {
                    Console.Clear();
                    ConsoleSystem.SetColor(Color.BlueViolet);
                    ConsoleSystem.AnimatedText("Downloading update...");
                    DownloadSystem.ServerDownload(DownloadSystem.AppFiles["astro"], "Updater\\AstroClient.exe");
                    DownloadSystem.ServerDownload(DownloadSystem.AppFiles["astroRuntime"], "Updater\\AstroClient.dll");
                    DownloadSystem.ServerDownload(DownloadSystem.AppFiles["astroRuntimeConfig"], "Updater\\AstroClient.runtimeconfig.json");
                    DownloadSystem.ServerDownload(DownloadSystem.AppFiles["astroDeps"], "Updater\\AstroClient.deps.json");
                    ConsoleSystem.AnimatedText("Update downloaded successfully. Restarting...");
                    LogSystem.Log("Update downloaded successfully.");
                    string cmdScript = $"@echo off\r\n" +
                           $"timeout /t 1 /nobreak > nul\r\n" +
                           $"xcopy /Y /Q \"Updater\\*.*\" \"%~dp0\" > nul\r\n" +
                           $"start \"\" \"%~dp0AstroClient.exe\" AppUpdating\r\n" +
                           $"exit";


                    string cmdScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StartApp.cmd");

                    File.WriteAllText(cmdScriptPath, cmdScript);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = cmdScriptPath,
                        UseShellExecute = true,
                        CreateNoWindow = true
                    });
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    HandleDownloadError(ex);
                }
            }
        }
        private static void HandleDownloadError(Exception ex)
        {
            ConsoleSystem.SetColor(Color.DarkRed);
            ConsoleSystem.AnimatedText($"Error downloading update. Please report the log file to Astro.");
            LogSystem.ReportError($"Error downloading update: {ex}");
            Task.Delay(2000).Wait();
            Environment.Exit(1);
        }
        // DEPRECATED
        private static void ReplaceCurrentExecutable()
        {
            string currentExecutable = Assembly.GetExecutingAssembly().Location;
            string currentRuntime = "AstroClient.dll";
            string currentRuntimeConfig = "AstroClient.runtimeconfig.json";
            string currentDeps = "AstroClient.deps.json";
            string updatedExecutable = "update.exe";
            string updatedRuntime = "runtime.dll";
            string updatedRuntimeConfig = "runtimeconfig.json";
            string updatedDeps = "deps.json";

            LogSystem.Log("Preparing to replace the current executable with the update.");

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c title Astro Updater && cls && timeout /T 1 /nobreak && " +
                                $"move /Y \"{updatedExecutable}\" \"{currentExecutable}\" && " +
                                $"move /Y \"{updatedRuntime}\" \"{currentRuntime}\" && " +
                                $"move /Y \"{updatedRuntimeConfig}\" \"{currentRuntimeConfig}\" && " +
                                $"move /Y \"{updatedDeps}\" \"{currentDeps}\" && " +
                                "echo Update Complete, You may restart Astro... (Closing in 3s) && " +
                                "timeout /T 3 /nobreak",
                    CreateNoWindow = false,
                    UseShellExecute = false
                };
                Process.Start(psi);
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception ex)
            {
                HandleUpdateError(ex);
            }
        }
        private static void HandleUpdateError(Exception ex)
        {
            ConsoleSystem.SetColor(Color.DarkRed);
            ConsoleSystem.AnimatedText($"Error applying update. Please report the log file to Astro.");
            LogSystem.ReportError($"Error applying update: {ex}");
            Environment.Exit(1);
        }
    }

}
