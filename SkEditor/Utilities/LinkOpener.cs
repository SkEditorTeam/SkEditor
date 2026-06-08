using System;
using System.Diagnostics;
using System.IO;

namespace SkEditor.Utilities;

// todo: move to a service
public static class LinkOpener
{
    /// <summary>
    ///     Opens the specified URL in the default web browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public static void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
    }

    /// <summary>
    ///     Opens the specified folder in the system's file explorer.
    /// </summary>
    /// <param name="path">The path of the folder to open.</param>
    public static void OpenFolder(string path)
    {
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
            Process.Start("open", path);
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", path);
        }
    }

    /// <summary>
    ///     Opens the folder containing the specified file and selects the file in the file explorer.
    /// </summary>
    /// <param name="filePath"></param>
    /// <remarks>Supported on Windows and macOS. On Linux, it will open the folder but not select the file.</remarks>
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