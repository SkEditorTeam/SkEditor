using SkEditor.API;
using SkEditor.Views;
using System;
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

        // exception is '--crash <base64 encoded exception>'
        try
        {
            var rawException = args[1];
            var exception = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(rawException));

            var tempPath = Path.Combine(Path.GetTempPath(), "SkEditor");
            if (Directory.Exists(tempPath))
            {
                var files = Directory.GetFiles(tempPath).ToList();
                if (files.Count != 0)
                    files.ForEach(file => SkEditorAPI.Files.OpenFile(file));
                Directory.Delete(tempPath, true);
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
}
