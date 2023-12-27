using AstroClient.Systems;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Client
{
    internal class UpdateManager
    {
        public static Version Version = new Version("3.0.3");
        public static Dictionary<string, string> AppFiles = new Dictionary<string, string>()
        {
            { "astro", "https://cdn.astroswrld.club/Client/AstroClient.exe" },
            { "astroRuntime", "https://cdn.astroswrld.club/Client/AstroClient.dll" },
            { "modpack", "https://cdn.astroswrld.club/Client/modpack.zip" },
            { "shader", "https://cdn.astroswrld.club/Client/RetroShader.ini" },
            { "instructions", "https://cdn.astroswrld.club/Client/shaderinstructions.txt" },
            { "reshade", "https://reshade.me/downloads/ReShade_Setup_5.9.2.exe" },
        };
        public static async Task CheckAppUpdates()
        {
            LogSystem.Log("Starting update check.");
            ConsoleSystem.UpdateArt();

            ConsoleSystem.AnimatedText($"Current Version: {Version} | Latest Version: {ServerManager.latestVersion}");
            LogSystem.Log($"Current Version: {Version} | Latest Version: {ServerManager.latestVersion}");

            if (Version < ServerManager.latestVersion)
            {
                ConsoleSystem.AnimatedText("Update Available!");
                LogSystem.Log("Update Available!");
                ConsoleSystem.AnimatedText("Downloading Update...");
                LogSystem.Log("Downloading Update...");
                await DownloadAppUpdate();
            }
            else
            {
                ConsoleSystem.AnimatedText("No Update Available.");
                LogSystem.Log("No Update Available.");
            }
        }

        public static async Task CheckModUpdates()
        {
            LogSystem.Log("Starting mod update check.");
            ConsoleSystem.UpdateArt();
        }

        public static async Task DownloadAppUpdate()
        {
            try
            {
                LogSystem.Log("Starting application update process.");

                using (WebClient client = new WebClient())
                {
                    try
                    {
                        client.DownloadFile(AppFiles["astro"], "update.exe");
                        client.DownloadFile(AppFiles["astroRuntime"], "runtime.dll");
                        Console.WriteLine("Update downloaded successfully.");
                        LogSystem.Log("Update downloaded successfully.");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error downloading update, Please report to Astro: {ex.InnerException}");
                        LogSystem.ReportError($"Error downloading update: {ex.InnerException}");
                        Thread.Sleep(10000);
                        return;
                    }
                }

                // Replace the current executable with the updated one
                string currentExecutable = Assembly.GetExecutingAssembly().Location;
                string currentRuntime = "AstroClient.dll";
                string updatedExecutable = "update.exe";
                string updatedRuntime = "runtime.dll";
                LogSystem.Log("Preparing to replace current executable with the update.");

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = "cmd.exe",
                        Arguments = $"/C timeout /T 1 /nobreak && " +
            $"move /Y \"{updatedExecutable}\" \"{currentExecutable}\" && " +
            $"move /Y \"{updatedRuntime}\" \"{currentRuntime}\"",
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    Process.Start(psi);
                    Process.GetCurrentProcess().Kill();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error applying update: {ex.Message}");
                    LogSystem.ReportError($"Error applying update: {ex.Message}");
                    return;
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Unexpected error in ForceAppUpdate: {ex.Message}");
            }
        }
    }
}
