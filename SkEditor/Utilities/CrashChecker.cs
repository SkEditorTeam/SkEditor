using SkEditor.API;
using SkEditor.Views;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace SkEditor.Utilities;
public class CrashChecker
{
    public async static Task<bool> CheckForCrash()
    {
        var args = SkEditorAPI.Core.GetStartupArguments();
        if (args is not ["--crash", _])
            return false;

        try
        {
            var rawException = args[1];
            var exception = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(rawException));
            var tempPath = Path.Combine(Path.GetTempPath(), "SkEditor");
            if (Directory.Exists(tempPath))
            {
                var files = Directory.GetFiles(tempPath).ToList();
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
                foreach (var file in files)
                {
                    if (IsFileLocked(file))
                    {
                        allFilesUnlocked = false;
                        break;
                    }
                }
                if (!allFilesUnlocked)
                    await Task.Delay(100);
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
