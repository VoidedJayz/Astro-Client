using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AstroClient.Systems
{
    public static class LogSystem
    {
        private static readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
        private static readonly string logFilePath = GetSecureLogFilePath();
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private static bool isProcessing = false;

        public static async Task Start()
        {
            using (FileStream fs = File.Open(logFilePath, FileMode.Create))
            {
                fs.Close();
                FileSystem.AppendAllText(logFilePath, "=====================[APPLICATION BEGIN]=====================" + Environment.NewLine);
            }
            stopwatch.Start();
            Task.Run(ProcessLogQueue);
        }

        public static void Log(string message)
        {
            string logEntry = $"[{DateTime.Now}] [Elapsed: {FormatElapsedTime(stopwatch.Elapsed)}]: {message}";
            logQueue.Enqueue(logEntry);
        }

        public static void ReportError(string errorMessage)
        {
            string errorLogEntry = $"[ERROR {DateTime.Now}] [Elapsed: {FormatElapsedTime(stopwatch.Elapsed)}]: {errorMessage}";
            logQueue.Enqueue(errorLogEntry);
        }

        private static void ProcessLogQueue()
        {
            while (true)
            {
                if (logQueue.TryDequeue(out var logEntry))
                {
                    File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
                }
                else if (!isProcessing)
                {
                    Task.Delay(1000).Wait(); // Wait for 1 second if the queue is empty
                }
            }
        }


        private static string FormatElapsedTime(TimeSpan elapsed)
        {
            if (elapsed.TotalHours >= 1)
            {
                return $"{Math.Floor(elapsed.TotalHours)} hour{(elapsed.TotalHours >= 2 ? "s" : "")}";
            }
            else if (elapsed.TotalMinutes >= 1)
            {
                return $"{Math.Floor(elapsed.TotalMinutes)} minute{(elapsed.TotalMinutes >= 2 ? "s" : "")}";
            }
            else
            {
                return $"{Math.Floor(elapsed.TotalSeconds)} second{(elapsed.TotalSeconds >= 2 ? "s" : "")}";
            }
        }

        private static string GetSecureLogFilePath()
        {
            string logDir = Directory.GetCurrentDirectory();
            string logDirectory = $"{logDir}\\Logs";

            if (!FileSystem.DirectoryExists(logDirectory))
            {
                FileSystem.CreateDirectory(logDirectory);
            }

            return logDirectory + $"\\AstroLog.{DateTime.Now:MM-dd-yyyy}.txt";
        }
    }

}
