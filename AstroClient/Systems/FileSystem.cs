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
                LogSystem.ReportError($"Error reading file: {ex.Message}");
                return null;
            }
        }

        public static string CreateUniqueFolderName(string baseFolderName, string directoryPath)
        {
            string folderName = $"{baseFolderName}{DateTime.Now:yyyy-MM-dd}";
            string uniqueFolderName = folderName;
            int counter = 1;

            while (DirectoryExists(Path.Combine(directoryPath, uniqueFolderName)))
            {
                uniqueFolderName = $"{folderName}_{counter++}";
            }

            string folderPath = Path.Combine(directoryPath, uniqueFolderName);
            CreateDirectory(folderPath);
            return folderPath;
        }

        public static void WriteAllBytes(string filePath, byte[] bytes)
        {
            try
            {
                using (FileStream fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(bytes, 0, bytes.Length);
                }
                LogSystem.Log($"Bytes written to file: {bytes.Length} bytes to {filePath}");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error writing bytes to file '{filePath}': {ex.Message}");
            }
        }

        public static void CopyFile(string sourceFilePath, string destinationFilePath)
        {
            try
            {
                File.Copy(sourceFilePath, destinationFilePath, true);
            }
            catch (IOException ioEx)
            {
                LogSystem.ReportError($"An IO exception occurred: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An exception occurred: {ex.Message}");
            }
        }

        public static void CopyDirectory(string sourceDirPath, string destinationDirPath)
        {
            try
            {
                foreach (string dirPath in Directory.GetDirectories(sourceDirPath, "*", SearchOption.AllDirectories))
                {
                    Directory.CreateDirectory(dirPath.Replace(sourceDirPath, destinationDirPath));
                }

                foreach (string filePath in Directory.GetFiles(sourceDirPath, "*.*", SearchOption.AllDirectories))
                {
                    File.Copy(filePath, filePath.Replace(sourceDirPath, destinationDirPath), true);
                }
            }
            catch (IOException ioEx)
            {
                LogSystem.ReportError($"An IO exception occurred: {ioEx.Message}");
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"An exception occurred: {ex.Message}");
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
                LogSystem.ReportError($"Error getting directories: {ex.Message}");
                return new List<string>();
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
                LogSystem.ReportError($"Error getting files: {ex.Message}");
                return new List<string>();
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
                if (!FileExists(sourcePath))
                    throw new FileNotFoundException("Source file does not exist.", sourcePath);

                File.Move(sourcePath, destinationPath);
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error moving file: {ex.Message}");
            }
        }

        public static void DeleteFile(string filePath)
        {
            try
            {
                if (!FileExists(filePath))
                    throw new FileNotFoundException("File does not exist.", filePath);

                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error deleting file: {ex.Message}");
            }
        }

        public static void MoveDirectory(string sourcePath, string destinationPath)
        {
            try
            {
                if (!DirectoryExists(sourcePath))
                    throw new DirectoryNotFoundException("Source directory does not exist.");

                Directory.Move(sourcePath, destinationPath);
            }
            catch (Exception ex)
            {
                LogSystem.ReportError($"Error moving directory: {ex.Message}");
            }
        }

        public static void DeleteDirectory(string directoryPath)
        {
            try
            {
                if (!DirectoryExists(directoryPath))
                    throw new DirectoryNotFoundException("Directory does not exist.");

                Directory.Delete(directoryPath, true);
            }
            catch (Exception ex)
            {
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
                if (FileExists(filePath))
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
            List<string> lines = new List<string>(File.ReadAllLines(filePath));
            string pattern = $@"^\s*{key}\s*=\s*(.*)\s*$";

            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim() == $"[{section}]")
                {
                    for (int j = i + 1; j < lines.Count; j++)
                    {
                        Match match = Regex.Match(lines[j], pattern);

                        if (match.Success)
                        {
                            lines[j] = $"{key}={newValue}";
                            break;
                        }
                        else if (lines[j].StartsWith("["))
                        {
                            break;
                        }
                    }
                    break;
                }
            }

            File.WriteAllLines(filePath, lines);
        }

    }
}
