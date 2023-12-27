using AstroClient.Systems;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Client
{
    internal class MusicManager
    {
        public static string? songKey;
        public static string? songPath;
        public static bool musicStopped = false;
        public static bool rerollSong = false;
        public static bool prioritySong = false;
        public static Dictionary<string, string> music = new Dictionary<string, string>()
        {
             { "Drag Me Down by One Direction", "MenuMusic\\Drag_Me_Down.mp3" },
             { "FLAUNT by SUPXR", "MenuMusic\\Flaunt.mp3" },
             { "RAPTURE by INTERWORLD", "MenuMusic\\RAPTURE.mp3" },
             { "I am... All Of Me by Crush 40", "MenuMusic\\I_Am_All_Of_Me.mp3" },
             { "Painkiller by Three Days Grace", "MenuMusic\\Painkiller.mp3" },
             { "Messages From The Stars by The Rah Band", "MenuMusic\\messages_from_the_stars.mp3" },
             { "Candle Queen by Ghost and Pals", "MenuMusic\\Candle_Queen.mp3" },
             { "Just An Attraction by TryHardNinja", "MenuMusic\\Just_An_Attraction.mp3" },
             { "DNA by LXNGVX", "MenuMusic\\DNA_LXNGVX.mp3" }
        };
        public static Dictionary<string, string> musicIndex = new Dictionary<string, string>()
        {
             { "1", "Drag Me Down by One Direction" },
             { "2", "FLAUNT by SUPXR" },
             { "3", "RAPTURE by INTERWORLD" },
             { "4", "I am... All Of Me by Crush 40" },
             { "5", "Painkiller by Three Days Grace" },
             { "6", "Messages From The Stars by The Rah Band" },
             { "7", "Candle Queen by Ghost and Pals" },
             { "8", "Just An Attraction by TryHardNinja" },
             { "9", "DNA by LXNGVX" }
        };
        public static async Task Start()
        {
            LogSystem.Log("Checking for priority song.");
            CheckPrioritySong();

            try
            {
                var audioFile = new AudioFileReader(songPath);
                LogSystem.Log($"Starting playback for file: {songPath}");

                using (var outputDevice = new WaveOutEvent())
                {
                    outputDevice.Init(audioFile);
                    outputDevice.PlaybackStopped += (sender, e) =>
                    {
                        LogSystem.Log("Playback stopped.");

                        if (rerollSong)
                        {
                            LogSystem.Log("Rerolling song due to reroll request.");
                            return;
                        }
                        if (prioritySong)
                        {
                            LogSystem.Log("Maintaining priority song.");
                            return;
                        }
                        if (ConfigSystem.loadedConfig.backgroundMusic)
                        {
                            outputDevice.Stop();
                            audioFile.Dispose();
                            ReRoll();
                            audioFile = new AudioFileReader(songPath);
                            outputDevice.Init(audioFile);
                            outputDevice.Play();
                            LogSystem.Log("Restarting playback with new song after reroll.");
                        }
                    };
                    outputDevice.Volume = 0.6f;
                    LogSystem.Log("Audio playback volume set.");

                    while (true)
                    {
                        if (rerollSong)
                        {
                            outputDevice.Stop();
                            audioFile.Dispose();
                            ReRoll();
                            audioFile = new AudioFileReader(songPath);
                            outputDevice.Init(audioFile);
                            rerollSong = false;
                            LogSystem.Log("Song rerolled and playback restarted.");
                        }
                        if (prioritySong)
                        {
                            outputDevice.Stop();
                            audioFile.Dispose();
                            CheckPrioritySong();
                            audioFile = new AudioFileReader(songPath);
                            outputDevice.Init(audioFile);
                            prioritySong = false;
                            LogSystem.Log("Priority song updated and playback restarted.");
                        }
                        else if (!musicStopped)
                        {
                            await Task.Delay(1000);
                            if (ConfigSystem.loadedConfig.backgroundMusic == true)
                            {
                                outputDevice.Play();
                                string formattedCurrentPosition = String.Format("{0:mm\\:ss}", audioFile.CurrentTime);
                                string formattedEndPosition = String.Format("{0:mm\\:ss}", audioFile.TotalTime);
                                Console.Title = $"ASTRO BOYZ! | {UpdateManager.Version} | ♫ {songKey} ♪ ({formattedCurrentPosition}/{formattedEndPosition})";
                            }
                            else
                            {
                                outputDevice.Pause();
                                Console.Title = $"ASTRO BOYZ! | {UpdateManager.Version} | ";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in SillyMusicHandler: {ex.Message}");
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
        private static void CheckPrioritySong()
        {
            if (ConfigSystem.loadedConfig.prioritySong != null)
            {
                if (music.ContainsKey(ConfigSystem.loadedConfig.prioritySong))
                {
                    songKey = ConfigSystem.loadedConfig.prioritySong;
                    songPath = music[songKey];
                    LogSystem.Log($"Priority song found: {songKey}");
                }
                else
                {
                    ReRoll();
                }
            }
            else
            {
                ReRoll();
            }
        }
        private static void ReRoll()
        {
            Random random = new Random();
            List<string> keys = new List<string>(music.Keys);
            int randomIndex;
            string newSongKey;

            do
            {
                randomIndex = random.Next(keys.Count);
                newSongKey = keys[randomIndex];
            } while (newSongKey == songKey);

            songKey = newSongKey;
            songPath = music[songKey];
            LogSystem.Log($"Song rerolled to: {songKey}");
        }
    }
}
