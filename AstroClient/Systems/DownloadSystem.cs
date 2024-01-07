using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Systems
{
    public class DownloadSystem
    {
        public static string cdn = "https://storage.bunnycdn.com/astroswrld/Client/";
        public static string accessKey = "50390255-a5f5-48c5-aab7bacdece6-7b3b-4ada";
        public static HttpClient client = new HttpClient();
        public static Dictionary<string, string> AppFiles = new Dictionary<string, string>()
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
        public static Dictionary<string, string> dependencies = new Dictionary<string, string>()
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
        };

        public static void Start()
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
        }
        public static void ServerDownload(string link, string dest)
        {
            try
            {
                Task downloadTask = Task.Run(async () =>
                {

                    string downloadLink = $"{link}?accessKey={accessKey}";

                    HttpResponseMessage response = await client.GetAsync(downloadLink);

                    if (response.IsSuccessStatusCode)
                    {
                        using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                        {
                            using (FileStream fileStream = File.Create(dest))
                            {
                                await contentStream.CopyToAsync(fileStream);
                                LogSystem.Log($"[{dest}] Server Responded With: {response.StatusCode} {response.ReasonPhrase} {response.Headers.Date}", "Download System");
                            }
                        }
                    }
                    else
                    {
                        LogSystem.ReportError($"Error downloading {dest} | {response.StatusCode}", "Download System");
                    }
                    response.Dispose();
                });

                downloadTask.Wait();
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Unexpected error in ServerDownload: {ex}");
            }
        }
    }
}