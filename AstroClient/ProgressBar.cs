using AstroClient.Systems;
using System;
using System.Text;
using System.Threading;

namespace AstroClient
{
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const char progressCharacter = '█';
        private const char emptyProgressCharacter = '─'; // A lighter line for the empty part of the progress bar
        private const string startDelimiter = "│"; // A vertical bar for the start delimiter
        private const string endDelimiter = "│"; // A vertical bar for the end delimiter
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 30);
        private const string animation = @"|/-\";

        private readonly Timer timer;
        private double currentProgress = 0;
        private string currentText = string.Empty;
        private bool disposed = false;
        private int animationIndex = 0;

        public ProgressBar()
        {
            timer = new Timer(TimerHandler);
            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(double value)
        {
            value = Math.Max(0, Math.Min(100, value)); // Adjust the range to 0-100
            Interlocked.Exchange(ref currentProgress, value);
        }

        private void TimerHandler(object state)
        {
            lock (timer)
            {
                if (disposed) return;

                int consoleWidth = Console.WindowWidth - 10; // Adjusted for padding and percentage
                int consoleHeight = Console.WindowHeight;
                int progressBlockCount = (int)(currentProgress * consoleWidth);
                int percent = (int)(currentProgress * 100);
                string text = string.Format("\r{0,-3}% {1}{2}{3}{4} {5}",
                    percent,
                    startDelimiter,
                    new string(progressCharacter, progressBlockCount),
                    new string(emptyProgressCharacter, consoleWidth - progressBlockCount),
                    endDelimiter,
                    animation[animationIndex++ % animation.Length]);

                Console.SetCursorPosition(0, consoleHeight - 1); // Move to the last line
                ConsoleSystem.SetColor(System.Drawing.Color.DeepPink);
                Console.Write(text);
                Console.ResetColor();

                currentText = text;
                ResetTimer();
            }
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
                Console.SetCursorPosition(0, Console.WindowHeight - 1); // Move to the last line
                Console.Write("\r" + new string(' ', currentText.Length) + "\r"); // Clear the line
                timer.Dispose();
            }
        }
    }

}
