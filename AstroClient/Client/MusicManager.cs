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
        public static MusicLibrary library = new MusicLibrary();
        public static void Start()
        {
            LogSystem.Log("Starting music handler.");
            RegisterSongs();
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
                            outputDevice.Stop();
                            audioFile.Dispose();
                            ReRoll();
                            audioFile = new AudioFileReader(songPath);
                            outputDevice.Init(audioFile);
                            outputDevice.Play();
                            rerollSong = false;
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

                    outputDevice.Volume = ConfigSystem.loadedConfig.musicVolume;
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
                            Task.Delay(1000).Wait();
                            if (ConfigSystem.loadedConfig.backgroundMusic)
                            {
                                outputDevice.Volume = ConfigSystem.loadedConfig.musicVolume;
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
                LogSystem.ReportError($"Error in MusicHandler: {ex}");
                Console.WriteLine($"Error in MusicHandler. Check logs for details.");
            }
        }

        private static void RegisterSongs()
        {
            library.AddSong("Drag Me Down by One Direction", "MenuMusic\\Drag_Me_Down.mp3");
            library.AddSong("FLAUNT by SUPXR", "MenuMusic\\Flaunt.mp3");
            library.AddSong("RAPTURE by INTERWORLD", "MenuMusic\\RAPTURE.mp3");
            library.AddSong("I am... All Of Me by Crush 40", "MenuMusic\\I_Am_All_Of_Me.mp3");
            library.AddSong("Painkiller by Three Days Grace", "MenuMusic\\Painkiller.mp3");
            library.AddSong("Messages From The Stars by The Rah Band", "MenuMusic\\messages_from_the_stars.mp3");
            library.AddSong("Candle Queen by Ghost and Pals", "MenuMusic\\Candle_Queen.mp3");
            library.AddSong("Just An Attraction by TryHardNinja", "MenuMusic\\Just_An_Attraction.mp3");
            library.AddSong("DNA by LXNGVX", "MenuMusic\\DNA_LXNGVX.mp3");
            library.AddSong("Montagem Mysterious Game by LXNGVX", "MenuMusic\\Montagem_Mysterious_Game.mp3");
            library.AddSong("Stuck Inside by Black Gryph0n", "MenuMusic\\Stuck_Inside.mp3");
            library.AddSong("Numb by Linkin Park (B.E.D & Noise Machine Remix)", "MenuMusic\\Numb.mp3");
        }
        private static void CheckPrioritySong()
        {
            if (ConfigSystem.loadedConfig.prioritySong != null)
            {
                var priority = library.GetSongByName(ConfigSystem.loadedConfig.prioritySong);
                if (priority != null)
                {
                    songKey = priority.Name;
                    songPath = priority.Path;
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
            int songCount = library.SongCount();
            int randomIndex;
            Song newSong;

            do
            {
                randomIndex = random.Next(songCount);
                newSong = library.GetSongByNumber(randomIndex + 1);
            } while (newSong.Name == songKey);

            songKey = newSong.Name;
            songPath = newSong.Path;
            LogSystem.Log($"Song rerolled to: {songKey}");
        }
    }
    public class Song
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int Number { get; set; }

        public Song(string name, string path, int number)
        {
            Name = name;
            Path = path;
            Number = number;
        }
    }

    public class MusicLibrary
    {
        private List<Song> songs = new List<Song>();

        public void AddSong(string name, string path)
        {
            int number = songs.Count + 1;
            songs.Add(new Song(name, path, number));
        }

        public Song GetSongByName(string name)
        {
            return songs.FirstOrDefault(s => s.Name == name);
        }

        public Song GetSongByNumber(int number)
        {
            return songs.FirstOrDefault(s => s.Number == number);
        }
        public int SongCount()
        {
            return songs.Count;
        }

        public bool IsValidSongNumber(int number)
        {
            return number > 0 && number <= songs.Count;
        }

        public List<Song> GetAllSongs()
        {
            return new List<Song>(songs);
        }
    }
}
