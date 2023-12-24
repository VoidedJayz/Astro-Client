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
            ServerData data = await GetServerData();
            latestVersion = new Version(data.version);
            currentChangelog = data.changelogs;
            currentStatus = data.status;

            if (currentStatus == "Offline")
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Astro Server is currently offline. The application will close in 5s.");
                await Task.Delay(5000);
                Environment.Exit(0);
            }
        }
        public static async Task<ServerData> GetServerData()
        {
            Console.WriteLine("Checking Astro Server Stats..");
            try
            {
                var client = new HttpClient();
                var response = await client.GetAsync(endPoint);
                var result = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<ServerData>(result);
                Console.WriteLine("Successfully received Astro Server Stats.");
                return data;
            }
            catch
            {
                Console.WriteLine("Failed to get Astro Server Stats.. The application will close in 5s.");
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
