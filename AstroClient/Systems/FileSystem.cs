using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AstroClient.Systems
{
    internal class FileSystem
    {
        public static void AppendAllText(string filePath, string content)
        {
            try
            {
                File.AppendAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error appending to file: {ex.Message}");
                LogSystem.ReportError($"Error appending to file: {ex.Message}");
            }
        }
        public static void WriteAllText(string filePath, string content)
        {
            try
            {
                File.WriteAllText(filePath, content);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error writing to file: {ex.Message}");
                LogSystem.ReportError($"Error writing to file: {ex.Message}");
            }
        }
        public static string ReadAllText(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("File does not exist.", filePath);

                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
                LogSystem.ReportError($"Error reading file: {ex.Message}");
                return null;
            }
        }
        public static List<string> GetDirectories(string path)
        {
            try
            {
                if (!DirectoryExists(path))
                    throw new DirectoryNotFoundException($"Directory does not exist: {path}");

                var directories = new List<string>(Directory.GetDirectories(path));
                return directories;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting directories: {ex.Message}");
                LogSystem.ReportError($"Error getting directories: {ex.Message}");
                return new List<string>(); // Return an empty list in case of an error
            }
        }
        public static List<string> GetFiles(string directoryPath, string searchPattern = "*.*", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            try
            {
                if (!DirectoryExists(directoryPath))
                    throw new DirectoryNotFoundException($"Directory does not exist: {directoryPath}");

                var files = new List<string>(Directory.GetFiles(directoryPath, searchPattern, searchOption));
                return files;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting files: {ex.Message}");
                LogSystem.ReportError($"Error getting files: {ex.Message}");
                return new List<string>(); // Return an empty list in case of an error
            }
        }
        public static void CreateDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (IOException ioEx)
                {
                    LogSystem.ReportError($"IO error: {ioEx.Message}");
                }
                catch (UnauthorizedAccessException unAuthEx)
                {
                    LogSystem.ReportError($"Unauthorized Access: {unAuthEx.Message}");
                }
                catch (System.Exception ex)
                {
                    LogSystem.ReportError($"Error creating directory: {ex.Message}");
                }
            }
        }
        public static void MoveFile(string sourcePath, string destinationPath)
        {
            try
            {
                if (!File.Exists(sourcePath))
                    throw new FileNotFoundException("Source file does not exist.", sourcePath);

                File.Move(sourcePath, destinationPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving file: {ex.Message}");
                LogSystem.ReportError($"Error moving file: {ex.Message}");
            }
        }
        public static void DeleteFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    throw new FileNotFoundException("File does not exist.", filePath);

                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting file: {ex.Message}");
                LogSystem.ReportError($"Error deleting file: {ex.Message}");
            }
        }
        public static void MoveDirectory(string sourcePath, string destinationPath)
        {
            try
            {
                if (!Directory.Exists(sourcePath))
                    throw new DirectoryNotFoundException("Source directory does not exist.");

                Directory.Move(sourcePath, destinationPath);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error moving directory: {ex.Message}");
                LogSystem.ReportError($"Error moving directory: {ex.Message}");
            }
        }
        public static void DeleteDirectory(string directoryPath)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                    throw new DirectoryNotFoundException("Directory does not exist.");

                Directory.Delete(directoryPath, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting directory: {ex.Message}");
                LogSystem.ReportError($"Error deleting directory: {ex.Message}");
            }
        }
        public static bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }
        public static bool DirectoryExists(string directoryPath)
        {
            return Directory.Exists(directoryPath);
        }
        public static void CreateFile(string filePath)
        {
            try
            {

                if (File.Exists(filePath))
                {
                    LogSystem.Log($"File already exists: {filePath}");
                    return;
                }

                using (FileStream fs = File.Create(filePath))
                {
                    LogSystem.Log($"File created: {filePath}");
                }
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error creating file: {ex.Message}");
            }
        }
        public static void ReplaceIniValue(string filePath, string section, string key, string newValue)
        {
            // Read all lines from the file
            List<string> lines = new List<string>(File.ReadAllLines(filePath));

            // Create a regular expression pattern to match the key within the specified section
            string pattern = $@"^\s*{key}\s*=\s*(.*)\s*$";

            // Iterate through the lines and find the key within the specified section
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim() == $"[{section}]")
                {
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        Match match = Regex.Match(lines[j], pattern);

                        if (match.Success)
                        {
                            // Replace the old value with the new value
                            lines[j] = $"{key}={newValue}";
                            break;
                        }
                        else if (lines[j].StartsWith("["))
                        {
                            // If a new section is encountered, break the inner loop
                            break;
                        }
                    }
                    break;
                }
            }

            // Write the modified lines back to the file
            File.WriteAllLines(filePath, lines);
        }

    }
}
