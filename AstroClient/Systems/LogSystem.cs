using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AstroClient.Systems
{
    public static class LogSystem
    {
        public static string LatestLogPath => GetLogFilePath("ApplicationLog-Latest.html");
        private static readonly ConcurrentQueue<string> logQueue = new ConcurrentQueue<string>();
        private static readonly string logDirectory = GetLogDirectory();
        private static readonly Stopwatch stopwatch = new Stopwatch();
        private static bool isProcessing = false;

        public static void Start()
        {
            CreateLogDirectory();

            // Find the next available log number
            int nextLogNumber = 1;
            while (FileSystem.FileExists(GetLogFilePath($"ApplicationLog-{nextLogNumber}.html")) && nextLogNumber <= 5)
            {
                nextLogNumber++;
            }

            // Remove Misplaced Logs
            var mainDir = Directory.GetCurrentDirectory() + "\\Logs";
            if (Directory.Exists(mainDir))
            {
                var files = FileSystem.GetFiles(mainDir, "*.html", SearchOption.TopDirectoryOnly);
                foreach (string file in files)
                {
                    FileSystem.DeleteFile(file);
                }
            }

            // Rename the current "Latest" log if it exists
            string currentLatestLogPath = GetLogFilePath("ApplicationLog-Latest.html");
            if (FileSystem.FileExists(currentLatestLogPath))
            {
                FileSystem.MoveFile(currentLatestLogPath, GetLogFilePath($"ApplicationLog-{nextLogNumber}.html"));
            }

            string htmlHeader = "<!DOCTYPE html>\n<html>\n<head>\n" +
                            "<style>\n" +
                            "  body {\n" +
                            "    background-color: black;\n" +
                            "    font-family: 'Segoe UI', sans-serif;\n" +
                            "    font-size: 16px;\n" +
                            "  }\n" +
                            "  .timestamp {\n" +
                            "    color: rgb(173, 216, 230);\n" +
                            "  }\n" +
                            "  .elapsed {\n" +
                            "    color: rgb(255, 247, 200);\n" +
                            "  }\n" +
                            "  .log-class {\n" +
                            "    color: rgb(0, 200, 155);\n" +
                            "  }\n" +
                            "  .log {\n" +
                            "    color: darkgray;\n" +
                            "  }\n" +
                            "  .error {\n" +
                            "    color: rgb(255, 0, 0);\n" +
                            "  }\n" +
                            "</style>\n" +
                            "</head>\n<body>\n" +
                            "<pre>";


            FileSystem.WriteAllText(GetLogFilePath("ApplicationLog-Latest.html"), htmlHeader);

            stopwatch.Start();
            Task.Run(ProcessLogQueue);
        }

        public static void Log(string message, string overrideClass = null)
        {
            string getClass()
            {
                if (overrideClass != null)
                {
                    return overrideClass;
                }
                else
                {
                    return GetCallingClassName();
                }
            }
            string callingClass = getClass();
            string color = GetSoftColorForClass(callingClass);
            string logEntry = $"<span class='timestamp'>[{DateTime.Now:hh:mm tt}]</span> " +
                              $"<span class='elapsed'>[Elapsed: {FormatElapsedTime(stopwatch.Elapsed)}]</span> " +
                              $"<span class='log'>- {message}</span>" +
                              $"<span class='log-class' style='color: {color}; float: right;'>[{callingClass}]</span> ";
            logQueue.Enqueue(logEntry);
        }

        public static void ReportError(string errorMessage, string overrideClass = null)
        {
            string getClass()
            {
                if (overrideClass != null)
                {
                    return overrideClass;
                }
                else
                {
                    return GetCallingClassName();
                }
            }
            string callingClass = getClass();
            string color = GetSoftColorForClass(callingClass);
            string errorLogEntry = $"<span class='timestamp'>[{DateTime.Now:hh:mm tt}]</span> " +
                                   $"<span class='elapsed'>[Elapsed: {FormatElapsedTime(stopwatch.Elapsed)}]</span> " +
                                   $"<span class='error'>- [ERROR] {errorMessage}</span>" +
                                   $"<span class='log-class' style='color: {color}; float: right;'>[{callingClass}]</span> ";
            logQueue.Enqueue(errorLogEntry);
        }


        private static void CreateLogDirectory()
        {
            if (!FileSystem.DirectoryExists(logDirectory))
            {
                FileSystem.CreateDirectory(logDirectory);
            }
        }

        private static string GetLogDirectory()
        {
            string logDir = Directory.GetCurrentDirectory();
            return $"{logDir}\\Logs\\{DateTime.Now:MM-dd-yy}-Entries";
        }

        private static string GetLogFilePath(string logFileName)
        {
            return Path.Combine(logDirectory, logFileName);
        }

        private static string GetSoftColorForClass(string className)
        {
            int hash = className.GetHashCode();
            int hue = hash & 0xFF;
            int saturation = 60;
            int lightness = 75;
            string color = HslToRgb(hue, saturation, lightness);
            return color;
        }

        public static string HslToRgb(int hue, int saturation, int lightness)
        {
            float sat = saturation / 100f;
            float light = lightness / 100f;
            float C = (1 - Math.Abs(2 * light - 1)) * sat;
            float X = C * (1 - Math.Abs((hue / 60f) % 2 - 1));
            float m = light - C / 2;
            float rPrime, gPrime, bPrime;

            if (hue < 60)
            {
                rPrime = C;
                gPrime = X;
                bPrime = 0;
            }
            else if (hue < 120)
            {
                rPrime = X;
                gPrime = C;
                bPrime = 0;
            }
            else if (hue < 180)
            {
                rPrime = 0;
                gPrime = C;
                bPrime = X;
            }
            else if (hue < 240)
            {
                rPrime = 0;
                gPrime = X;
                bPrime = C;
            }
            else if (hue < 300)
            {
                rPrime = X;
                gPrime = 0;
                bPrime = C;
            }
            else
            {
                rPrime = C;
                gPrime = 0;
                bPrime = X;
            }

            int r = (int)((rPrime + m) * 255);
            int g = (int)((gPrime + m) * 255);
            int b = (int)((bPrime + m) * 255);

            return $"#{r:X2}{g:X2}{b:X2}";
        }
        private static void ProcessLogQueue()
        {
            while (true)
            {
                if (logQueue.TryDequeue(out var logEntry))
                {
                    // Append the log entry to the HTML file
                    FileSystem.AppendAllText(GetLogFilePath("ApplicationLog-Latest.html"), logEntry + "\n");
                }
                else if (!isProcessing)
                {
                    Task.Delay(1000).Wait(); // Wait for 1 second if the queue is empty
                }
            }
        }

        private static string GetCallingClassName()
        {
            StackTrace stackTrace = new StackTrace();
            for (int i = 1; i < stackTrace.FrameCount; i++)
            {
                StackFrame frame = stackTrace.GetFrame(i);
                MethodBase method = frame.GetMethod();
                Type declaringType = method.DeclaringType;
                if (declaringType != null && declaringType != typeof(LogSystem))
                {
                    string className = declaringType.Name;
                    // Insert a space before each capital letter (except the first one)
                    className = Regex.Replace(className, "(?<!^)([A-Z])", " $1");
                    return className;
                }
            }
            return "Unknown";
        }


        private static string FormatElapsedTime(TimeSpan elapsed)
        {
            string formattedTime = $"{elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}";
            return formattedTime;
        }
    }
}
