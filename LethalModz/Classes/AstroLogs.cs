using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LethalModz.Classes
{
    public static class AstroLogs
    {
        private static Stopwatch stopwatch = new Stopwatch();
        private static string logFilePath = "StartupLog.txt";

        public static void Start()
        {
            stopwatch.Start();
            if (AstroFileSystem.FileExists(logFilePath))
            {
                AstroFileSystem.DeleteFile(logFilePath);
            }
            Log("Application Start");
        }

        public static void Log(string message)
        {
            var logEntry = $"{DateTime.Now} - {message} - Elapsed: {stopwatch.Elapsed}";
            AstroFileSystem.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }
    } 
}
