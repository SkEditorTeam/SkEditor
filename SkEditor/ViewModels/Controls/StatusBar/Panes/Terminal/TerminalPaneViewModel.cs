using System;
using System.Diagnostics;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Iciclecreek.Terminal;
using SkEditor.Utilities.Localization;

namespace SkEditor.ViewModels.Controls.StatusBar.Panes.Terminal;

public class TerminalPaneViewModel : ObservableObject, IDrawerPaneViewModel, IDisposable
{
    public TranslatableString Name => TranslatableString.FromRawString("Terminal");
    public IDrawerToolbarViewModel? Toolbar { get; }

    public TerminalControl TerminalControl => field ??= CreateTerminal();

    public TerminalPaneViewModel() => Toolbar = new TerminalToolbarViewModel(this);

    private TerminalControl CreateTerminal()
    {
        TerminalControl control = new()
        {
            BufferSize = 1000,
            Background = Brushes.Transparent,
            Foreground = Brush.Parse("#cccccc"),
            CloseProcessOnDetach = false
        };

        if (OperatingSystem.IsWindows())
        {
            control.Process = IsCommandAvailable("pwsh.exe") ? "pwsh.exe" : "powershell.exe";
        }
        else
        {
            control.Process = Environment.GetEnvironmentVariable("SHELL") ?? "bash";
        }

        _ = control.LaunchProcess();

        return control;
    }

    private static bool IsCommandAvailable(string command)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "where" : "which",
                Arguments = command,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            })?.WaitForExit();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        TerminalControl?.Kill();
    }
}