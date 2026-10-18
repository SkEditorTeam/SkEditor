using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Iciclecreek.Terminal;
using R3;
using SkEditor.Core.Localization;
using SkEditor.Layout.StatusBar.UI.Abstractions;
using ExperimentsConfig = SkEditor.Configuration.Configs.ExperimentsConfig;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Layout.StatusBar.UI.Panes.Terminal;

public class TerminalPaneViewModel : ObservableObject, IDrawerPaneViewModel, IDisposable
{
    private TerminalControl? _terminalControl;
    public string Id => "Terminal";
    public BindableReactiveProperty<bool> IsVisible { get; }
    public TranslatableString Name => TranslationKey.BottomDrawerTerminalTitle;
    public TerminalControl TerminalControl => _terminalControl ??= CreateTerminal();
    public IDrawerToolbarViewModel? Toolbar { get; }

    public TerminalPaneViewModel(ExperimentsConfig experiments)
    {
        Toolbar = new TerminalToolbarViewModel(this);

        IsVisible = experiments
            .ObservePropertyChanged(x => x.TerminalEnabled)
            .ToBindableReactiveProperty();
    }

    public async Task RestartTerminal()
    {
        await TerminalControl.ClearAsync();
        await TerminalControl.LaunchProcess();
    }

    public async Task Clear()
    {
        await TerminalControl.ClearAsync();
    }

    private TerminalControl CreateTerminal()
    {
        TerminalControl control = new()
        {
            BufferSize = 1000,
            Background = Brushes.Transparent,
            Foreground = Brush.Parse("#cccccc"),
            CloseProcessOnDetach = false,
            StartingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)
        };

        if (OperatingSystem.IsWindows())
        {
            bool isNewPowershellAvailable = IsCommandAvailable("pwsh.exe");
            control.Process = isNewPowershellAvailable ? "pwsh.exe" : "powershell.exe";

            if (isNewPowershellAvailable)
            {
                const string ps1Init =
                    "function Clear-Host { [Console]::Out.Write(\"$([char]27)]2;__CLEAR_SCROLLBACK__$([DateTime]::Now.Ticks)$([char]7)\"); [Console]::Out.Write(\"$([char]27)[2J$([char]27)[H\"); };";
                control.Args = ["-NoLogo", "-NoExit", "-Command", ps1Init];
            }
        }
        else
        {
            control.Process = Environment.GetEnvironmentVariable("SHELL") ?? "bash";
        }

        _ = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await control.ClearAsync();
            await control.LaunchProcess();
        });

        return control;
    }

    private static bool IsCommandAvailable(string command)
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = OperatingSystem.IsWindows() ? "where" : "which",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process? process = Process.Start(startInfo);
            if (process == null)
            {
                return false;
            }

            process.WaitForExit();

            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _terminalControl?.Kill();
    }
}