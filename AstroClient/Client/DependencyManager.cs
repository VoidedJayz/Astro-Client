using AstroClient.Systems;
using System.Net;

namespace AstroClient.Client
{
    internal class DependencyManager
    {
        public static Dictionary<string, string> dependencies = new Dictionary<string, string>()
        {
            { "Colorful.Console.dll", "https://cdn.astroswrld.club/Client/Dependencies/Colorful.Console.dll" },
            { "NAudio.dll", "https://cdn.astroswrld.club/Client/Dependencies/NAudio.dll" },
            { "NAudio.Core.dll", "https://cdn.astroswrld.club/Client/Dependencies/NAudio.Core.dll" },
            { "NAudio.Wasapi.dll", "https://cdn.astroswrld.club/Client/Dependencies/NAudio.Wasapi.dll" },
            { "NAudio.WinMM.dll", "https://cdn.astroswrld.club/Client/Dependencies/NAudio.WinMM.dll" },
            { "NAudio.Asio.dll",  "https://cdn.astroswrld.club/Client/Dependencies/NAudio.Asio.dll" },
            { "NAudio.Midi.dll", "https://cdn.astroswrld.club/Client/Dependencies/NAudio.Midi.dll" },
            { "DiscordRPC.dll", "https://cdn.astroswrld.club/Client/Dependencies/DiscordRPC.dll" },
            { "Newtonsoft.Json.dll", "https://cdn.astroswrld.club/Client/Dependencies/Newtonsoft.Json.dll" },
            { "MenuMusic\\FLAUNT.mp3", "https://cdn.astroswrld.club/Client/Dependencies/FLAUNT.mp3" },
            { "MenuMusic\\Drag_Me_Down.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Drag_Me_Down.mp3" },
            { "MenuMusic\\RAPTURE.mp3", "https://cdn.astroswrld.club/Client/Dependencies/RAPTURE.mp3" },
            { "MenuMusic\\I_Am_All_Of_Me.mp3", "https://cdn.astroswrld.club/Client/Dependencies/I_Am_All_Of_Me.mp3" },
            { "MenuMusic\\Painkiller.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Painkiller.mp3" },
            { "MenuMusic\\messages_from_the_stars.mp3", "https://cdn.astroswrld.club/Client/Dependencies/messages_from_the_stars.mp3" },
            { "MenuMusic\\Candle_Queen.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Candle_Queen.mp3" },
            { "MenuMusic\\Just_An_Attraction.mp3", "https://cdn.astroswrld.club/Client/Dependencies/Just_An_Attraction.mp3" },
            { "MenuMusic\\DNA_LXNGVX.mp3", "https://cdn.astroswrld.club/Client/Dependencies/DNA_LXNGVX.mp3" }
        };

        public static async Task CheckDependencies()
        {
            try
            {
                LogSystem.Log("Checking dependencies..");
                if (!FileSystem.DirectoryExists("MenuMusic"))
                {
                    FileSystem.CreateDirectory("MenuMusic");
                }
                foreach (KeyValuePair<string, string> dependency in dependencies)
                {
                    if (!FileSystem.FileExists(dependency.Key))
                    {
                        LogSystem.Log($"Dependency {dependency.Key} not found. Downloading..");
                        ConsoleSystem.AnimatedText($"Downloading dependency {dependency.Key}..");
                        await DownloadDependency(dependency.Key, dependency.Value);
                    }
                    else
                    {
                        LogSystem.Log($"Dependency {dependency.Key} found.");
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in CheckDependencies: {ex.Message}");
            }
        }

        private static Task DownloadDependency(string key, string value)
        {
            try
            {
                using (WebClient client = new())
                {
                    client.DownloadFile(value, key);
                }
                LogSystem.Log($"Dependency {key} downloaded.");
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in DownloadDependency: {ex.Message}");
                return Task.CompletedTask;
            }
        }
    }
}
