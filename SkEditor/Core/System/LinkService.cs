using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Core.Logging;

namespace SkEditor.Core.System;

public class LinkService(ILogService logs) : ILinkService
{
    public void OpenLink(string url)
    {
        bool isUri = Uri.TryCreate(url, UriKind.Absolute, out Uri? uri);

        if (!isUri || uri is null || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            logs.Warning($"Invalid or unsupported link: '{url}'.");
            return;
        }

        Process.Start(new ProcessStartInfo(uri.AbsoluteUri) { UseShellExecute = true })?.Dispose();
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
            })?.Dispose();
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.StartAndForget("open", [path]);
        }
        else if (OperatingSystem.IsLinux())
        {
            Process.StartAndForget("xdg-open", [path]);
        }
    }

    public async ValueTask OpenFolderWithFileSelectionAsync(string filePath)
    {
        string fullPath = Path.GetFullPath(filePath);
        
        if (!File.Exists(fullPath))
        {
            logs.Warning($"Attempted to open folder with file selection for '{filePath}', but the file doesn't exist.");
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            Process.StartAndForget(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{fullPath}\""
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            Process.StartAndForget("open", ["-R", fullPath]);
        }
        else if (OperatingSystem.IsLinux())
        {
            string fileUri = new Uri(fullPath).AbsoluteUri;

            try
            {
                using CancellationTokenSource cts = new(TimeSpan.FromSeconds(2));
                
                ProcessExitStatus status = await Process.RunAsync("dbus-send", [
                    "--session",
                    "--dest=org.freedesktop.FileManager1",
                    "--type=method_call",
                    "--print-reply",
                    "/org/freedesktop/FileManager1",
                    "org.freedesktop.FileManager1.ShowItems",
                    $"array:string:{fileUri}",
                    "string:"
                    ],
                    silent: true,
                    cancellationToken: cts.Token);

                if (status.Canceled || status.ExitCode != 0)
                {
                    FallbackOpen(fullPath);
                }
            }
            catch (Exception ex)
            {
                logs.Warning($"Failed to open folder with file selection for '{filePath}' using dbus-send: {ex.Message}");
                FallbackOpen(fullPath);
            }
        }
        else
        {
            FallbackOpen(fullPath);
        }
    }

    private void FallbackOpen(string filePath)
    {
        string? folderPath = Path.GetDirectoryName(filePath);
        if (folderPath is not  null)
        {
            OpenFolder(folderPath);
        }
    }
}