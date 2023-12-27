using AstroClient.Systems;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AstroClient.Objects;

namespace AstroClient.Client
{
    internal class ServerManager
    {
        public static string endPoint = "https://api.astroswrld.club/v1/get-info";
        public static string? currentStatus;
        // Astro Data
        public static Version? latestVersion;
        public static Version? latestModpackVersion;
        public static string? currentChangelog;
        public static string? currentModpackChangelog;

        public static async Task SetupData()
        {
            try
            {
                LogSystem.Log("Starting SetupData.");

                ServerData data = await GetServerData();
                latestVersion = new Version(data.version);
                currentChangelog = data.changelogs;
                currentStatus = data.status;
                latestModpackVersion = new Version(data.modpackVersion);
                currentModpackChangelog = data.modpackChangelogs;

                LogSystem.Log($"Astro data retrieved. Latest version: {latestVersion}, Status: {currentStatus}");
                LogSystem.Log($"Astro Modpack data retrieved. Latest version: {latestModpackVersion}, Status: {currentStatus}");

                if (currentStatus == "Offline")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Astro Server is currently offline. The application will close in 5s.");
                    LogSystem.Log("Astro Server is offline. Exiting application.");
                    await Task.Delay(5000);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to contact Astros Server.. The application will close in 5s.");
                LogSystem.ReportError($"Error in SetupData: {ex.Message}");
                await Task.Delay(5000);
                Environment.Exit(0);
            }
        }
        public static async Task<ServerData> GetServerData()
        {
            LogSystem.Log("Checking Astro Server Stats..");
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(endPoint);
                var result = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ServerData>(result);

                LogSystem.Log("Successfully received Astro Server Stats.");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get Astro Server Stats.. The application will close in 5s.");
                LogSystem.ReportError($"Error getting server stats: {ex.Message}. Exiting application.");
                await Task.Delay(5000);
                Environment.Exit(0);
                return null;
            }
        }
    }
}
