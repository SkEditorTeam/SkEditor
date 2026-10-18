using System;
using System.Diagnostics;
using System.IO;
using SkEditor.Core.Logging;

namespace SkEditor.Core.System;

public class LinkService(ILogService logs) : ILinkService
{
    public void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    public void OpenFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            logs.Warning($"Attempted to open folder '{path}', but it doesn't exist.");
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            ProcessStartInfo psi = new("open");
            psi.ArgumentList.Add(path);
            Process.Start(psi);
        }
        else if (OperatingSystem.IsLinux())
        {
            ProcessStartInfo psi = new("xdg-open");
            psi.ArgumentList.Add(path);
            Process.Start(psi);
        }
    }

    public void OpenFolderWithFileSelection(string filePath)
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
            ProcessStartInfo psi = new("open");
            psi.ArgumentList.Add("-R");
            psi.ArgumentList.Add(filePath);
            Process.Start(psi);
        }
        else if (OperatingSystem.IsLinux() && CheckDbusExisting())
        {
            Process.Start("dbus-send",
                $"--session --dest=org.freedesktop.FileManager1 --type=method_call --print-reply /org/freedesktop/FileManager1 org.freedesktop.FileManager1.ShowItems array:string:\"file://{filePath}\" string:\"\"");
        }
        else
        {
            string? folderPath = Path.GetDirectoryName(filePath);
            if (folderPath == null)
            {
                throw new ArgumentException("Invalid file path");
            }

            OpenFolder(folderPath);
        }
    }

    private static bool CheckDbusExisting()
    {
        try
        {
            Process.Start("dbus-send",
                "--session --dest=org.freedesktop.DBus --type=method_call --print-reply / org.freedesktop.DBus.ListNames");
            return true;
        }
        catch
        {
            return false;
        }
    }
}