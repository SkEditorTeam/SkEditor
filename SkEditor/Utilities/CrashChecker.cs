using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Files;
using SkEditor.Services.Windowing;
using CrashWindow = SkEditor.Views.Windows.CrashWindow;
using Path = System.IO.Path;

namespace SkEditor.Utilities;

public class CrashChecker(
    ILogService logService,
    IStartupService startupService,
    WindowRegistry windowRegistry,
    IFileService fileService)
{
    public async Task<bool> CheckForCrash()
    {
        string[]? args = startupService.StartupArguments;
        if (args is not ["--crash", _])
        {
            return false;
        }

        try
        {
            string rawException = args[1];
            string exception = Encoding.UTF8.GetString(Convert.FromBase64String(rawException));
            string tempPath = Path.Combine(Path.GetTempPath(), "SkEditor");
            if (Directory.Exists(tempPath))
            {
                List<string> files = Directory.GetFiles(tempPath).ToList();
                if (files.Count != 0)
                {
                    foreach (string file in files)
                    {
                        await fileService.OpenFileAsync(new LocalFile(file));
                    }

                    await WaitForUnlock(files);

                    try
                    {
                        Directory.Delete(tempPath, true);
                    }
                    catch (Exception ex)
                    {
                        logService.Warning($"Failed to delete temporary directory: {ex}");
                    }
                }
            }

            if (windowRegistry.Active is { } mainWindow)
            {
                await new CrashWindow(exception).ShowDialog(mainWindow);
            }

            return true;
        }
        catch (FormatException e)
        {
            logService.Warning($"Failed to decode crash exception: {e}");
            return false;
        }
    }

    private static async Task WaitForUnlock(List<string> files)
    {
        await Task.Run(async () =>
        {
            bool allFilesUnlocked = false;
            while (!allFilesUnlocked)
            {
                allFilesUnlocked = files.All(file => !IsFileLocked(file));

                if (!allFilesUnlocked)
                {
                    await Task.Delay(100);
                }
            }
        });
    }

    private static bool IsFileLocked(string filePath)
    {
        try
        {
            using FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
            stream.Close();
        }
        catch (IOException)
        {
            return true;
        }

        return false;
    }
}