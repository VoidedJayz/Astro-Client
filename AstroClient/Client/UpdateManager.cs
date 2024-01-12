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
        public static Version Version = new Version("3.2.7");
        public static void Start()
        {
            if (Program.DebuggerMode)
            {
                return;
            }
            LogSystem.Log("Starting update check.");
            ConsoleSystem.UpdateArt();
            ConsoleSystem.SetColor(Color.Gold);
            ConsoleSystem.AnimatedText($"Current Astro Version: {Version} | Latest Astro Version: {ServerManager.latestVersion}");
            LogSystem.Log($"Current Astro Version: {Version} | Latest Astro Version: {ServerManager.latestVersion}");

            if (Version < ServerManager.latestVersion)
            {
                ConsoleSystem.SetColor(Color.Cyan);
                ConsoleSystem.AnimatedText("Update Available!");
                LogSystem.Log("Update Available!");
                if (ConfigSystem.loadedConfig.autoUpdateAstro)
                {
                    ConsoleSystem.AnimatedText("Auto Update Enabled, Downloading Update...");
                    LogSystem.Log("Auto Update Enabled, Downloading Update...");
                    DownloadAppUpdate();
                }
                else
                {
                    ConsoleSystem.AnimatedText("Auto Update Disabled, Press any key to continue...");
                    LogSystem.Log("Auto Update Disabled, Press any key to continue...");
                    Console.ReadKey();
                }
            }
            else
            {
                ConsoleSystem.SetColor(Color.LimeGreen);
                ConsoleSystem.AnimatedText("No Updates Available for Astro.");
                LogSystem.Log("No Updates Available for Astro.");
                Task.Delay(200).Wait();
            }
            ConsoleSystem.UpdateArt();
            ConsoleSystem.SetColor(Color.Gold);
            ConsoleSystem.AnimatedText($"Current Modpack Version: {ModManager.Version} | Latest Modpack Version: {ServerManager.latestModpackVersion}");
            LogSystem.Log($"Current Modpack Version: {ModManager.Version} | Latest Modpack Version: {ServerManager.latestModpackVersion}");
            if (ModManager.Version < ServerManager.latestModpackVersion)
            {
                ConsoleSystem.SetColor(Color.Cyan);
                ConsoleSystem.AnimatedText("There is a modpack update available!");
                ConsoleSystem.AnimatedText("We highly recommend you update the modpack using Astro.");
                ConsoleSystem.AnimatedText("If you do not update the modpack, you will not be properly synced in game.");
                ConsoleSystem.AnimatedText("These updates are extremely important for peer to peer syncing and bug fixes.");
                ConsoleSystem.AnimatedText("Press any key to continue...");
                LogSystem.Log("Modpack Update Available!");
                Console.ReadKey();
            }
            else
            {
                ConsoleSystem.SetColor(Color.LimeGreen);
                ConsoleSystem.AnimatedText("No Updates Available for Modpack.");
                LogSystem.Log("No Updates Available for Modpack.");
                Task.Delay(200).Wait();
            }
        }

        public static void DownloadAppUpdate()
        {
            try
            {
                LogSystem.Log("Starting application update process.");

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        Console.Clear();
                        ConsoleSystem.SetColor(Color.BlueViolet);
                        ConsoleSystem.AnimatedText("Downloading update...");
                        DownloadSystem.ServerDownload(DownloadSystem.AppFiles["astro"], "update.exe");
                        DownloadSystem.ServerDownload(DownloadSystem.AppFiles["astroRuntime"], "runtime.dll");
                        DownloadSystem.ServerDownload(DownloadSystem.AppFiles["astroRuntimeConfig"], "runtimeconfig.json");
                        DownloadSystem.ServerDownload(DownloadSystem.AppFiles["astroDeps"], "deps.json");
                        ConsoleSystem.AnimatedText("Update downloaded successfully.");
                        LogSystem.Log("Update downloaded successfully.");
                    }
                    catch (Exception ex)
                    {
                        ConsoleSystem.SetColor(Color.DarkRed);
                        ConsoleSystem.AnimatedText($"Error downloading update, Please report the log file to Astro.");
                        LogSystem.ReportError($"Error downloading update: {ex}");
                        Task.Delay(2000).Wait();
                        return;
                    }
                }

                // Replace the current executable with the updated one
                string currentExecutable = Assembly.GetExecutingAssembly().Location;
                string currentRuntime = "AstroClient.dll";
                string currentRuntimeConfig = "AstroClient.runtimeconfig.json";
                string currentDeps = "AstroClient.deps.json";
                string updatedExecutable = "update.exe";
                string updatedRuntime = "runtime.dll";
                string updatedRuntimeConfig = "runtimeconfig.json";
                string updatedDeps = "deps.json";
                LogSystem.Log("Preparing to replace current executable with the update.");

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

                    ConsoleSystem.AnimatedText($"Error applying update, Please report the log file to Astro.");
                    LogSystem.ReportError($"Error applying update: {ex}");
                    return;
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Unexpected error in ForceAppUpdate: {ex}");
            }
        }
    }
}
