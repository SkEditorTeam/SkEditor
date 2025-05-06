using System;
using System.IO;
using System.Threading.Tasks;
using Serilog;

namespace SkEditor.Utilities.Files;

public class SessionFileHandler
{
    public static void EnsureSessionDirectoryExists()
    {
        try
        {
            if (!Directory.Exists(SessionRestorer.SessionFolder))
            {
                Directory.CreateDirectory(SessionRestorer.SessionFolder);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create session directory");
            throw;
        }
    }
    
    public static async Task CreateLockFile()
    {
        try
        {
            await File.WriteAllTextAsync(SessionRestorer.LockFilePath, DateTime.UtcNow.ToString("o"));
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to create lock file");
        }
    }
    
    public static async Task SafelyClearSessionFolder()
    {
        if (!Directory.Exists(SessionRestorer.SessionFolder))
        {
            return;
        }

        try
        {
            Directory.Delete(SessionRestorer.SessionFolder, true);
            await Task.Delay(100);
            Directory.CreateDirectory(SessionRestorer.SessionFolder);
        }
        catch (IOException ex)
        {
            Log.Warning(ex, "Could not clear session folder, trying to delete individual files");
            
            try
            {
                foreach (string file in Directory.GetFiles(SessionRestorer.SessionFolder))
                {
                    try
                    {
                        if (Path.GetFileName(file) != SessionRestorer.LockFileName)
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception innerEx)
                    {
                        Log.Warning(innerEx, "Failed to delete file: {File}", file);
                    }
                }
            }
            catch (Exception fallbackEx)
            {
                Log.Error(fallbackEx, "Failed to clear session folder using fallback method");
                throw;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to clear session folder");
            throw;
        }
    }
    
    public static string[] GetSessionFiles()
    {
        try
        {
            return !Directory.Exists(SessionRestorer.SessionFolder) ? [] : Directory.GetFiles(SessionRestorer.SessionFolder, $"*{SessionRestorer.SessionFileExtension}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to get session files");
            return [];
        }
    }
    
    public static async Task WriteFile(string path, string content)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("Path cannot be null or empty", nameof(path));
        }

        try
        {
            string tempPath = path + ".tmp";
            await File.WriteAllTextAsync(tempPath, content);
            
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            
            File.Move(tempPath, path);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to write file: {Path}", path);
            throw;
        }
    }
    
    public static async Task<string> ReadFile(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return string.Empty;
        }

        try
        {
            return await File.ReadAllTextAsync(path);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to read file: {Path}", path);
            return string.Empty;
        }
    }
    
    public static bool IsPathValid(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }

        try
        {
            if (path.IndexOfAny(Path.GetInvalidPathChars()) >= 0)
            {
                return false;
            }
            
            string extension = Path.GetExtension(path);
            return !string.IsNullOrEmpty(extension);
        }
        catch
        {
            return false;
        }
    }
}