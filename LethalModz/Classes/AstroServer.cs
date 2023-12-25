using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Astro.Classes;
using Newtonsoft.Json;

namespace LethalModz.Classes
{
    public class AstroServer
    {
        public static string endPoint = "https://api.astroswrld.club/v1/get-info";
        public static Version latestVersion;
        public static string currentChangelog;
        public static string currentStatus;

        public static async Task SetupData()
        {
            try
            {
                AstroLogs.Log("Starting SetupData.");

                ServerData data = await GetServerData();
                latestVersion = new Version(data.version);
                currentChangelog = data.changelogs;
                currentStatus = data.status;

                AstroLogs.Log($"Server data retrieved. Latest version: {latestVersion}, Status: {currentStatus}");

                if (currentStatus == "Offline")
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Astro Server is currently offline. The application will close in 5s.");
                    AstroLogs.Log("Astro Server is offline. Exiting application.");
                    await Task.Delay(5000);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                AstroLogs.Log($"Error in SetupData: {ex.Message}");
            }
        }
        public static async Task<ServerData> GetServerData()
        {
            AstroLogs.Log("Checking Astro Server Stats..");
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(endPoint);
                var result = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ServerData>(result);

                AstroLogs.Log("Successfully received Astro Server Stats.");
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get Astro Server Stats.. The application will close in 5s.");
                AstroLogs.Log($"Error getting server stats: {ex.Message}. Exiting application.");
                await Task.Delay(5000);
                Environment.Exit(0);
                return null;
            }
        }

    }

    public class ServerData
    {
        public string version { get; set; }
        public string changelogs { get; set; }
        public string status { get; set; }
    }
}
