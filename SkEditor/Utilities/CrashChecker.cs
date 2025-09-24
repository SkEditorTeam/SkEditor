using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkEditor.API;
using SkEditor.Views;
using CrashWindow = SkEditor.Views.Windows.CrashWindow;
using Path = System.IO.Path;

namespace SkEditor.Utilities;

public class CrashChecker
{
    public static async Task<bool> CheckForCrash()
    {
        string[] args = SkEditorAPI.Core.GetStartupArguments();
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
                    files.ForEach(file => SkEditorAPI.Files.OpenFile(file));
                    await WaitForUnlock(files);

                    try
                    {
                        Directory.Delete(tempPath, true);
                    }
                    catch (Exception ex)
                    {
                        SkEditorAPI.Logs.Warning($"Failed to delete temporary directory: {ex}");
                    }
                }
            }

            await SkEditorAPI.Windows.ShowWindowAsDialog(new CrashWindow(exception));
            return true;
        }
        catch (FormatException e)
        {
            SkEditorAPI.Logs.Warning($"Failed to decode crash exception: {e}");
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
                allFilesUnlocked = true;
                foreach (string file in files)
                {
                    if (IsFileLocked(file))
                    {
                        allFilesUnlocked = false;
                        break;
                    }
                }

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