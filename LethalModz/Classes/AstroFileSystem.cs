using LethalModz.Classes;
using System;
using System.IO;
using System.Security.Principal;

internal class AstroFileSystem
{
    private static bool IsAdmin()
    {
        using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
        {
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }
    }
    public static void AppendAllText(string filePath, string content)
    {
        try
        {
            if (!IsAdmin())
                throw new UnauthorizedAccessException("Administrator privileges required.");

            File.AppendAllText(filePath, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error appending to file: {ex.Message}");
            AstroLogs.Log($"Error appending to file: {ex.Message}");
        }
    }
    public static void WriteAllText(string filePath, string content)
    {
        try
        {
            if (!IsAdmin())
                throw new UnauthorizedAccessException("Administrator privileges required.");

            File.WriteAllText(filePath, content);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error writing to file: {ex.Message}");
            AstroLogs.Log($"Error writing to file: {ex.Message}");
        }
    }
    public static string ReadAllText(string filePath)
    {
        try
        {
            if (!IsAdmin())
                throw new UnauthorizedAccessException("Administrator privileges required.");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File does not exist.", filePath);

            return File.ReadAllText(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}");
            AstroLogs.Log($"Error reading file: {ex.Message}");
            return null;
        }
    }
    public static void MoveFile(string sourcePath, string destinationPath)
    {
        try
        {
            if (!IsAdmin())
                throw new UnauthorizedAccessException("Administrator privileges required.");
            if (!File.Exists(sourcePath))
                throw new FileNotFoundException("Source file does not exist.", sourcePath);

            File.Move(sourcePath, destinationPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving file: {ex.Message}");
            AstroLogs.Log($"Error moving file: {ex.Message}");
        }
    }
    public static void DeleteFile(string filePath)
    {
        try
        {
            if (!IsAdmin())
                throw new UnauthorizedAccessException("Administrator privileges required.");
            if (!File.Exists(filePath))
                throw new FileNotFoundException("File does not exist.", filePath);

            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting file: {ex.Message}");
            AstroLogs.Log($"Error deleting file: {ex.Message}");
        }
    }
    public static void MoveDirectory(string sourcePath, string destinationPath)
    {
        try
        {
            if (!IsAdmin())
                throw new UnauthorizedAccessException("Administrator privileges required.");
            if (!Directory.Exists(sourcePath))
                throw new DirectoryNotFoundException("Source directory does not exist.");

            Directory.Move(sourcePath, destinationPath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving directory: {ex.Message}");
            AstroLogs.Log($"Error moving directory: {ex.Message}");
        }
    }
    public static void DeleteDirectory(string directoryPath)
    {
        try
        {
            if (!IsAdmin())
                throw new UnauthorizedAccessException("Administrator privileges required.");
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException("Directory does not exist.");

            Directory.Delete(directoryPath, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting directory: {ex.Message}");
            AstroLogs.Log($"Error deleting directory: {ex.Message}");
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
            if (!IsAdmin())
                throw new UnauthorizedAccessException("Administrator privileges required.");

            if (File.Exists(filePath))
            {
                AstroLogs.Log($"File already exists: {filePath}");
                return;
            }

            using (FileStream fs = File.Create(filePath))
            {
                AstroLogs.Log($"File created: {filePath}");
            }
        }
        catch (Exception ex)
        {
            AstroLogs.Log($"Error creating file: {ex.Message}");
        }
    }
}
