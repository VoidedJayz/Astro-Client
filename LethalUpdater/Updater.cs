using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LethalUpdater
{
    internal class Updater
    {
        public static WebClient server = new WebClient();
        public static HttpClient server2 = new HttpClient();
        public static string GenerateSecureDownload(string url, int expires)
        {
            url = "https://api.astroswrld.club/api/v1/request-token?url=" + url + "&expires=" + expires + "&pureRes=true";

            var response = server.DownloadString(url);
            var resp = response.Split('"');
            return resp[1];
        }
        static void Main(string[] args)
        {
            var verions = server.DownloadString(GenerateSecureDownload("https://cdn.astroswrld.club/secure/Versions/version", 10));
            var updaterVers = verions.Split('|')[0];
            var astroVers = verions.Split('|')[1];
            server.Dispose();
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Title = $"LethalUpdater v{updaterVers}";
            Console.WriteLine($"LethalUpdater v{updaterVers}");
            Console.WriteLine("By: Astro Boyz");
            Console.WriteLine("Begin Update..");
            Console.SetWindowSize(50, 10);

            using (var progress = new ProgressBar())
            {
                try
                {
                    File.Delete("LethalModz.exe");
                }
                catch { }
                progress.Report(30);
                Thread.Sleep(1000);
                using (var client = new WebClient())
                {
                    var dl = GenerateSecureDownload("https://cdn.astroswrld.club/secure/Files/LethalModz.exe", 25);
                    client.DownloadFile(dl, "LethalModz.exe");
                    while (client.IsBusy)
                    {
                        Thread.Sleep(1000);
                    }
                    progress.Report(100);
                }
            }
            Process.Start("LethalModz.exe");
            server.Dispose();
            Environment.Exit(0);
        }

    }
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int blockCount = 35;
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private const string animation = @"|/-\";

        private readonly Timer timer;

        private double currentProgress = 0;
        private string currentText = string.Empty;
        private bool disposed = false;
        private int animationIndex = 0;

        public ProgressBar()
        {
            timer = new Timer(TimerHandler);

            // A progress bar is only for temporary display in a console window.
            // If the console output is redirected to a file, draw nothing.
            // Otherwise, we'll end up with a lot of garbage in the target file.
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(double value)
        {
            // Make sure value is in [0..1] range
            value = Math.Max(0, Math.Min(1, value));
            Interlocked.Exchange(ref currentProgress, value);
        }

        private void TimerHandler(object state)
        {
            lock (timer)
            {
                if (disposed) return;

                int progressBlockCount = (int)(currentProgress * blockCount);
                int percent = (int)(currentProgress * 100);
                string text = string.Format("[{0}{1}] {2,3}% {3}",
                    new string('#', progressBlockCount), new string('-', blockCount - progressBlockCount),
                    percent,
                    animation[animationIndex++ % animation.Length]);
                UpdateText(text);

                ResetTimer();
            }
        }

        private void UpdateText(string text)
        {
            // Get length of common portion
            int commonPrefixLength = 0;
            int commonLength = Math.Min(currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }

            // Backtrack to the first differing character
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.Substring(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            int overlapCount = currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            Console.Write(outputBuilder);
            currentText = text;
        }

        private void ResetTimer()
        {
            timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (timer)
            {
                disposed = true;
                UpdateText(string.Empty);
            }
        }

    }
}