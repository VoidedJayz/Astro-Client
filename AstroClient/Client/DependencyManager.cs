using AstroClient.Systems;
using Newtonsoft.Json;
using System.Drawing;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using static System.Windows.Forms.LinkLabel;

namespace AstroClient.Client
{
    internal class DependencyManager
    {
        public static int UpdatedCount = 0;
        public static int MissingCount = 0;
        public static bool ColorfulExists = false;
        public static string LastDependencyChecked = "";
        public static HttpClient client = new HttpClient();
        public static Dictionary<string, string> awaitingUpdates = new Dictionary<string, string>();

        public static void CheckDependencies()
        {
            try
            {
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                LogSystem.Log("Checking Dependencies..");
                if (!FileSystem.DirectoryExists("MenuMusic"))
                {
                    FileSystem.CreateDirectory("MenuMusic");
                }

                int index = 1;
                StringBuilder outputBuilder = new StringBuilder();
                outputBuilder.AppendLine("┌─────────────────────────────────────────────────────────────────────────────────┐");
                LogSystem.Log("┌─────────────────────────────────────────────────────────────────────────────────┐");
                Console.WriteLine("Checking Dependencies [Server Side]..");
                using (ProgressBar progress = new())
                {
                    progress.Report(0);
                    foreach (KeyValuePair<string, string> dependency in DownloadSystem.dependencies)
                    {
                        string fileName = Path.GetFileName(dependency.Key);
                        if (fileName == null)
                        {
                            continue;
                        }
                        if (fileName == "Colorful.Console.dll")
                        {
                            ColorfulExists = true;
                            continue;
                        }
                        LastDependencyChecked = fileName;
                        bool fileExists = FileSystem.FileExists(dependency.Key);
                        DateTime localLastModified = DateTime.MinValue;
                        DateTime serverLastModified = DateTime.MinValue;
                        (localLastModified, serverLastModified) = Task.Run(() => GetLocalAndServerLastModifiedAsync(dependency.Key, dependency.Value)).Result;
                        bool isOutdated = IsLocalFileVersionOutdated(localLastModified, serverLastModified);

                        string status = "";
                        if (!fileExists)
                        {
                            status = "Not Found";
                            MissingCount++;
                            DownloadDependency(dependency.Key, dependency.Value, serverLastModified);
                        }
                        else if (isOutdated)
                        {
                            status = "Outdated  ";
                            UpdatedCount++;
                            DownloadDependency(dependency.Key, dependency.Value, serverLastModified);
                        }
                        else
                        {
                            status = "Up To Date";
                            if (fileName == "Colorful.Console.dll")
                            {
                                status = "Skipped ";
                            }
                        }
                        var i = index++;
                        var s = $"│ {i,2} │ {Truncate(fileName, 24),-24} │ {status,-10} │ Local: {localLastModified.ToString("MM-dd-yy") + $" │ Server: {serverLastModified.ToString("MM-dd-yy")}",-10} │";

                        outputBuilder.AppendLine(s);
                        LogSystem.Log(s);
                        progress.Report((double)index / DownloadSystem.dependencies.Count);
                    }
                }
                outputBuilder.AppendLine("└─────────────────────────────────────────────────────────────────────────────────┘");
                LogSystem.Log("└─────────────────────────────────────────────────────────────────────────────────┘");
                string output = outputBuilder.ToString();
                Console.Clear();
                if (ColorfulExists)
                {
                    Colorful.Console.WriteWithGradient(output, Color.BlueViolet, Color.White, 10);
                }
                else
                {
                    Console.WriteLine(output);
                }

                LogSystem.Log("Done checking dependencies.");
                LogSystem.Log($"Updated {UpdatedCount} dependencies.");
                LogSystem.Log($"Downloaded Missing {MissingCount} dependencies.");
                if (ColorfulExists)
                {
                    Colorful.Console.WriteLine($"Updated {UpdatedCount} dependencies.");
                    Colorful.Console.WriteLine($"Downloaded Missing {MissingCount} dependencies.");
                    Colorful.Console.WriteLine("Press any key to continue..");
                }
                else
                {
                    Console.WriteLine($"Updated {UpdatedCount} dependencies.");
                    Console.WriteLine($"Downloaded {MissingCount} Missing dependencies.");
                    Console.WriteLine("Press any key to continue..");
                }
                Console.ReadKey();
                Console.Clear();
                Colorful.Console.ResetColor();
                Colorful.Console.ReplaceAllColorsWithDefaults();
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Dependency At Fault: {LastDependencyChecked}");
                LogSystem.ReportError($"Error in CheckDependencies: {ex}");
            }
        }

        public static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
        public static async Task<(DateTime localLastModified, DateTime serverLastModified)> GetLocalAndServerLastModifiedAsync(string localFilePath, string serverUrl)
        {
            DateTime localLastModified = DateTime.MinValue;
            DateTime serverLastModified = DateTime.MinValue;

            try
            {
                using (var client = new HttpClient())
                {
                    string downloadLink = $"{serverUrl}?accessKey={DownloadSystem.accessKey}";

                    HttpResponseMessage response = await client.GetAsync(downloadLink);

                    if (response.IsSuccessStatusCode)
                    {
                        localLastModified = File.GetLastWriteTimeUtc(localFilePath);

                        // Get the server's last modified date from the response headers
                        if (response.Content.Headers.TryGetValues("Last-Modified", out var values))
                        {
                            string lastModifiedHeader = values.FirstOrDefault();
                            if (!string.IsNullOrEmpty(lastModifiedHeader))
                            {
                                serverLastModified = DateTime.Parse(lastModifiedHeader).ToUniversalTime();
                            }
                        }
                    }
                    else
                    {
                        LogSystem.ReportError($"Error accessing server file: {response.StatusCode}");
                    }
                    response.Dispose();
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in GetLocalAndServerLastModifiedAsync: {ex}");
            }

            return (localLastModified, serverLastModified);
        }



        public static bool IsLocalFileVersionOutdated(DateTime local, DateTime server)
        {
            try
            {
                if (local < server)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error checking timestamp {ex}");
            }

            return false;
        }



        private static void DownloadDependency(string key, string value, DateTime timestamp)
        {
            // Simple Fix for timestamps
            try
            {
                DownloadSystem.ServerDownload(value, key);
                File.SetLastWriteTimeUtc(key, timestamp);

            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error in DownloadDependency: {ex}");
            }
        }
    }
}
