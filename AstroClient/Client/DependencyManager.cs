using AstroClient.Systems;
using Newtonsoft.Json;
using System.Diagnostics;
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

        public static void Start()
        {
            if (Program.VisualStudio)
            {
                return;
            }

            try
            {
                var completionTime = Stopwatch.StartNew();

                // Deprecation: Secure CDN message
                if (ServerManager.useSecureCDN)
                {
                    Console.WriteLine("Secure CDN is activated! All checks might take longer than usual.");
                    LogSystem.Log("Secure CDN is activated! All checks might take longer than usual.");
                }

                // Deprecation: Progress bar initialization
                Console.WriteLine("Checking Dependencies [Server Side]..");

                // Deprecation: User agent
                client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");
                LogSystem.Log("Checking Dependencies..");

                // Deprecation: MenuMusic directory creation
                if (!FileSystem.DirectoryExists("MenuMusic"))
                {
                    FileSystem.CreateDirectory("MenuMusic");
                }

                int index = 1;
                StringBuilder outputBuilder = new StringBuilder();
                outputBuilder.AppendLine("┌───────────────────────────────────────────────────────────────────────────────────────────────┐");

                using (ProgressBar progress = new())
                {
                    progress.Report(0);

                    foreach (KeyValuePair<string, string> dependency in DownloadSystem.dependencies)
                    {
                        string fileName = Path.GetFileName(dependency.Key);

                        // Deprecation: Colorful.Console and Newtonsoft.Json check
                        if (fileName == "Colorful.Console.dll" || fileName == "Newtonsoft.Json.dll")
                        {
                            ColorfulExists = true;
                            continue;
                        }

                        LastDependencyChecked = fileName;
                        bool fileExists = FileSystem.FileExists(dependency.Key);
                        long fileSize = 0;
                        DateTime localLastModified = DateTime.MinValue;

                        // Get the file size and last modified time locally
                        if (fileExists)
                        {
                            FileInfo fileInfo = new FileInfo(dependency.Key);
                            fileSize = fileInfo.Length;
                            localLastModified = fileInfo.LastWriteTime;
                        }

                        string formattedSize = FormatFileSize(fileSize);

                        string status = "";
                        if (!fileExists)
                        {
                            status = "Not Found";
                            MissingCount++;
                            DownloadDependency(dependency.Key, dependency.Value, DateTime.MinValue);
                        }
                        else
                        {
                            status = "Available";
                            if (fileName == "Colorful.Console.dll")
                            {
                                status = "Skipped ";
                            }
                        }

                        var i = index++;
                        var s = $"│ {i,2} │ {Truncate(fileName, 24),-24} │ {status,-10} │ Size: {formattedSize,-10} │ Modified: {localLastModified.ToString("MM-dd-yy │ HH:mm:ss"),-19} │";

                        outputBuilder.AppendLine(s);
                        LogSystem.Log($"{Truncate(fileName, 24),-24} {status,-10} Size: {formattedSize,-10} Modified: {localLastModified.ToString("MM-dd-yy HH:mm:ss"),-19}");
                        progress.Report((double)index / DownloadSystem.dependencies.Count);
                    }
                }

                outputBuilder.AppendLine("└───────────────────────────────────────────────────────────────────────────────────────────────┘");
                string output = outputBuilder.ToString();
                Console.Clear();

                // Deprecation: Colorful.Console output
                if (ColorfulExists)
                {
                    Colorful.Console.WriteWithGradient(output, Color.BlueViolet, Color.White, 10);
                }
                else
                {
                    Console.WriteLine(output);
                }

                completionTime.Stop();
                LogSystem.Log($"Done checking dependencies. Completed in {completionTime.Elapsed.TotalMilliseconds} MS.");
                LogSystem.Log($"Updated {UpdatedCount} dependencies.");
                LogSystem.Log($"Downloaded Missing {MissingCount} dependencies.");

                if (ColorfulExists)
                {
                    if (UpdatedCount > 0)
                    {
                        Colorful.Console.WriteLine($"Updated {UpdatedCount} dependencies.");
                    }
                    if (MissingCount > 0)
                    {
                        Colorful.Console.WriteLine($"Downloaded {MissingCount} Missing dependencies.");
                    }
                    Colorful.Console.WriteLine($"Took {Math.Floor(completionTime.Elapsed.TotalMilliseconds)} MS.");
                }
                else
                {
                    if (UpdatedCount > 0)
                    {
                        Console.WriteLine($"Updated {UpdatedCount} dependencies.");
                    }
                    if (MissingCount > 0)
                    {
                        Console.WriteLine($"Downloaded {MissingCount} Missing dependencies.");
                    }
                    Console.WriteLine("Press any key to continue..");
                }

                Task.Delay(1500).Wait();
                Console.Clear();

                // Deprecation: Colorful.Console reset
                Colorful.Console.ResetColor();
                Colorful.Console.ReplaceAllColorsWithDefaults();
                static string FormatFileSize(long sizeInBytes)
                {
                    const int KB = 1024;
                    const int MB = KB * 1024;
                    const int GB = MB * 1024;

                    if (sizeInBytes < KB)
                    {
                        return $"{sizeInBytes} B";
                    }
                    else if (sizeInBytes < MB)
                    {
                        return $"{(double)sizeInBytes / KB:F2} KB";
                    }
                    else if (sizeInBytes < GB)
                    {
                        return $"{(double)sizeInBytes / MB:F2} MB";
                    }
                    else
                    {
                        return $"{(double)sizeInBytes / GB:F2} GB";
                    }
                }
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
                    string downloadlink;
                    if (ServerManager.useSecureCDN)
                    {
                        downloadlink = $"{serverUrl}?accessKey={DownloadSystem.accessKey}";
                    }
                    else
                    {
                        downloadlink = serverUrl;
                    }

                    HttpResponseMessage response = await client.GetAsync(downloadlink);

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
