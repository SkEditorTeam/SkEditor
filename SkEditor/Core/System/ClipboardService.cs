using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using SkEditor.Contracts.Services;

namespace SkEditor.Services;

public class ClipboardService : IClipboardService
{
    public Task CopyTextAsync(string text)
    {
        return GetClipboard()?.SetTextAsync(text) ?? Task.CompletedTask;
    }

    public async Task<string?> GetTextAsync()
    {
        IClipboard? clipboard = GetClipboard();
        if (clipboard == null)
        {
            return null;
        }

        return await clipboard.TryGetTextAsync();
    }

    private static IClipboard? GetClipboard()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return TopLevel.GetTopLevel(desktop.MainWindow)?.Clipboard;
        }

        return null;
    }
}