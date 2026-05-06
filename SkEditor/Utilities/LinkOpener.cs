using System;
using System.Diagnostics;
using System.IO;

namespace SkEditor.Utilities;

public static class LinkOpener
{
    public static void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public static void OpenFolder(string path)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    public static void OpenFolderWithFileSelection(string filePath)
    {
        if (OperatingSystem.IsWindows())
        {
            string windowsPath = filePath.Replace("/", "\\");
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{windowsPath}\"",
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", $"-R \"{filePath}\"");
        }
        else
        {
            string? folderPath = Path.GetDirectoryName(filePath);
            if (folderPath == null)
            {
                throw new ArgumentException("Invalid file path");
            }

            Process.Start(new ProcessStartInfo(folderPath) { UseShellExecute = true });
        }
    }
}