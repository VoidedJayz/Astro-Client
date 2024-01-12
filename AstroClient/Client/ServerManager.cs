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
        public static bool useSecureCDN = false;
        // Astro Data
        public static Version? latestVersion;
        public static Version? latestModpackVersion;
        public static string? currentChangelog;
        public static string? currentModpackChangelog;

        public static void Start()
        {
            try
            {
                LogSystem.Log("Starting SetupData.");
                ServerData data = GetServerData();
                latestVersion = new Version(data.version);
                currentChangelog = data.changelogs;
                currentStatus = data.status;
                latestModpackVersion = new Version(data.modpackVersion);
                currentModpackChangelog = data.modpackChangelogs;
                if (currentStatus.ToLower().Contains("secure"))
                {
                    useSecureCDN = true;
                    LogSystem.Log("Astro Server is requesting to use the secure CDN.");
                }
                if (currentStatus.ToLower().Contains("offline"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Astro Server is currently offline. The application will close in 5s.");
                    LogSystem.Log("Astro Server is offline. Exiting application.");
                    Task.Delay(5000).Wait();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to contact Astros Server.. The application will close in 5s.");
                LogSystem.ReportError($"Error in SetupData: {ex}");
                Task.Delay(5000).Wait();
                Environment.Exit(0);
            }
        }
        public static ServerData GetServerData()
        {
            LogSystem.Log("Checking Astro Server Stats..");
            try
            {
                var client = new HttpClient();
                var response = client.GetAsync(endPoint).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    var data = JsonConvert.DeserializeObject<ServerData>(result);

                    LogSystem.Log("Successfully received Astro Server Stats.");
                    return data;
                }
                else
                {
                    LogSystem.ReportError($"Error getting server stats: HTTP {response.StatusCode}");
                    // Handle non-successful response here
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Failed to get Astro Server Stats.. The application will close in 5s.");
                LogSystem.ReportError($"Error getting server stats: {ex}. Exiting application.");
                Task.Delay(5000).Wait();
                Environment.Exit(0);
                return null;
            }
            return null;
        }
    }
}
