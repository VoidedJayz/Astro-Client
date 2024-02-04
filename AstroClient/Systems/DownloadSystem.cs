using AstroClient.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AstroClient.Systems
{
    public class DownloadSystem
    {
        public static string cdn = "Unknown";
        public static string accessKey = "50390255-a5f5-48c5-aab7bacdece6-7b3b-4ada"; // this is required to make the secure cdn return file data
        public static HttpClient client = new HttpClient();
        public static Dictionary<string, string> AppFiles = new Dictionary<string, string>();
        public static Dictionary<string, string> dependencies = new Dictionary<string, string>();

        public static void Start()
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd($"Astro Client App | Yo Mam Gei | {UpdateManager.Version}");
            if (ServerManager.useSecureCDN)
            {
                cdn = "https://storage.bunnycdn.com/astroswrld/Client/";
                LogSystem.Log($"Using Secure CDN {cdn}");
            }
            else
            {
                cdn = "http://cdn.astroswrld.club/Client/";
                LogSystem.Log($"Using General CDN {cdn}");
            }
            RefreshDependencies();
        }
        public static void ServerDownload(string link, string dest)
        {
            int maxRetries = 3;
            int retryDelayMilliseconds = 250;

            for (int retryCount = 0; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    LogSystem.Log($"Starting download: {link} -> {dest}", "Download System");

                    string downloadLink = ServerManager.useSecureCDN
                        ? $"{link}?accessKey={accessKey}"
                        : link;

                    HttpResponseMessage response;
                    var stopwatch = new System.Diagnostics.Stopwatch();

                    stopwatch.Start();
                    response = Task.Run(() => client.GetAsync(downloadLink)).Result;
                    stopwatch.Stop();

                    LogSystem.Log($"Time taken to get response: {stopwatch.ElapsedMilliseconds} ms", "Download System");

                    if (response.IsSuccessStatusCode)
                    {
                        using (Stream contentStream = Task.Run(() => response.Content.ReadAsStreamAsync()).Result)
                        {
                            long totalSizeBytes = response.Content.Headers.ContentLength ?? -1;
                            LogSystem.Log($"Total file size: {FormatFileSize(totalSizeBytes)}", "Download System");

                            using (FileStream fileStream = File.Create(dest))
                            {
                                contentStream.Seek(0, SeekOrigin.Begin);
                                contentStream.CopyTo(fileStream);
                            }

                            LogSystem.Log($"Download completed. Total bytes read: {totalSizeBytes}", "Download System");
                            LogSystem.Log($"[{dest}] Downloaded Successfully, Total Size: {FormatFileSize(totalSizeBytes)}", "Download System");
                            LogSystem.Log($"[{dest}] Server Responded With: {response.StatusCode} {response.ReasonPhrase} {response.Headers.Date}", "Download System");
                        }
                    }
                    else
                    {
                        LogSystem.ReportError($"Error downloading {dest} | {response.StatusCode}", "Download System");
                    }

                    response.Dispose();
                    break; // Successful download, break out of the retry loop
                }
                catch (Exception ex)
                {
                    if (retryCount < maxRetries - 1)
                    {
                        LogSystem.Log($"Retry {retryCount + 1}/{maxRetries}", "Download System");
                        Thread.Sleep(retryDelayMilliseconds);
                    }
                    else
                    {
                        LogSystem.ReportError($"Failed after {maxRetries} retries. Full Error message: {ex}", "Download System");
                    }
                }
            }
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1024 * 1024)
            {
                double sizeMB = (double)bytes / (1024 * 1024);
                return $"{sizeMB:F2} MB";
            }
            else if (bytes >= 1024)
            {
                double sizeKB = (double)bytes / 1024;
                return $"{sizeKB:F2} KB";
            }
            else
            {
                return $"{bytes} bytes";
            }
        }

        public static void RefreshDependencies()
        {
            AppFiles = new Dictionary<string, string>()
            {
                { "astro", $"{cdn}AstroClient.exe" },
                { "astroRuntime", $"{cdn}AstroClient.dll" },
                { "astroRuntimeConfig", $"{cdn}AstroClient.runtimeconfig.json" },
                { "astroDeps", $"{cdn}AstroClient.deps.json" },
                { "modpack", $"{cdn}modpack.zip" },
                { "shader", $"{cdn}RetroShader.ini" },
                { "instructions", $"{cdn}shaderinstructions.txt" },
                { "reshade", "https://reshade.me/downloads/ReShade_Setup_5.9.2.exe" },
            };
            dependencies = new Dictionary<string, string>()
            {
                // App Stuff
                { "Colorful.Console.dll", $"{cdn}Dependencies/Colorful.Console.dll" },
                { "NAudio.dll", $"{cdn}Dependencies/NAudio.dll" },
                { "NAudio.Core.dll", $"{cdn}Dependencies/NAudio.Core.dll" },
                { "NAudio.Wasapi.dll", $"{cdn}Dependencies/NAudio.Wasapi.dll" },
                { "NAudio.WinMM.dll", $"{cdn}Dependencies/NAudio.WinMM.dll" },
                { "NAudio.Asio.dll",  $"{cdn}Dependencies/NAudio.Asio.dll" },
                { "NAudio.Midi.dll", $"{cdn}Dependencies/NAudio.Midi.dll" },
                { "DiscordRPC.dll", $"{cdn}Dependencies/DiscordRPC.dll" },
                // Songs
                { "Newtonsoft.Json.dll", $"{cdn}Dependencies/Newtonsoft.Json.dll" },
                { "MenuMusic\\FLAUNT.mp3", $"{cdn}Dependencies/Music/FLAUNT.mp3" },
                { "MenuMusic\\Drag_Me_Down.mp3", $"{cdn}Dependencies/Music/Drag_Me_Down.mp3" },
                { "MenuMusic\\RAPTURE.mp3", $"{cdn}Dependencies/Music/RAPTURE.mp3" },
                { "MenuMusic\\I_Am_All_Of_Me.mp3", $"{cdn}Dependencies/Music/I_Am_All_Of_Me.mp3" },
                { "MenuMusic\\Painkiller.mp3", $"{cdn}Dependencies/Music/Painkiller.mp3" },
                { "MenuMusic\\messages_from_the_stars.mp3", $"{cdn}Dependencies/Music/messages_from_the_stars.mp3" },
                { "MenuMusic\\Candle_Queen.mp3", $"{cdn}Dependencies/Music/Candle_Queen.mp3" },
                { "MenuMusic\\Just_An_Attraction.mp3", $"{cdn}Dependencies/Music/Just_An_Attraction.mp3" },
                { "MenuMusic\\DNA_LXNGVX.mp3", $"{cdn}Dependencies/Music/DNA_LXNGVX.mp3" },
                { "MenuMusic\\Montagem_Mysterious_Game.mp3", $"{cdn}Dependencies/Music/Montagem_Mysterious_Game.mp3" },
                { "MenuMusic\\Stuck_Inside.mp3", $"{cdn}Dependencies/Music/Stuck_Inside.mp3" },
                { "MenuMusic\\Numb.mp3", $"{cdn}Dependencies/Music/Numb.mp3" },
                { "MenuMusic\\MYBAD.mp3", $"{cdn}Dependencies/Music/MYBAD.mp3" },
                { "MenuMusic\\TheDetailsInTheDevil.mp3", $"{cdn}Dependencies/Music/TheDetailsInTheDevil.mp3" },
                { "MenuMusic\\AnotherFiveNights.mp3", $"{cdn}Dependencies/Music/AnotherFiveNights.mp3" },
                { "MenuMusic\\METAMORPHOSIS.mp3", $"{cdn}Dependencies/Music/METAMORPHOSIS.mp3" },
                { "MenuMusic\\Unknown.mp3", $"{cdn}Dependencies/Music/Unknown.mp3" },
            };
        }
    }
}