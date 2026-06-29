using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Controls;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;

namespace SkEditor.Views.Controls.StatusBar.Panes;

public partial class TerminalPaneView : UserControl
{
    public TerminalPaneView()
    {
        InitializeComponent();
        InitializeComponentState();
    }

    [AvaloniaHotReload]
    private void InitializeComponentState()
    {
        if (OperatingSystem.IsWindows())
        {
            Terminal.Process = IsCommandAvailable("pwsh.exe") ? "pwsh.exe" : "powershell.exe";

            Terminal.Args = new List<string>
            {
                "-NoExit",
                "-Command",
                "function Clear-Host { Write-Host -NoNewline \"$([char]27)[2J$([char]27)[3J$([char]27)[H\" }; Clear-Host"
            };
        }
        else
        {
            App.Services.GetRequiredService<INotificationService>()
                .Debug($"Linux detected, shell: {Environment.GetEnvironmentVariable("SHELL")}");
            Terminal.Process = Environment.GetEnvironmentVariable("SHELL") ?? "bash";
        }
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
}