using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using Serilog;
using System;
using System.Diagnostics;

namespace SkEditor.Views;
public partial class TerminalWindow : AppWindow
{
    private Process _process;

    public TerminalWindow()
    {
        InitializeComponent();
        Focusable = true;

        LoadTerminal();
        AssignEvents();
    }

    private void LoadTerminal()
    {
        InitializeProcess();
        _process.Start();

        SetupProcessHandlers();
    }

    private void InitializeProcess()
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        };

        _process = new Process
        {
            StartInfo = startInfo,
            EnableRaisingEvents = true
        };
    }

    private void SetupProcessHandlers()
    {
        _process.OutputDataReceived += HandleOutput;
        _process.ErrorDataReceived += HandleOutput;

        _process.BeginOutputReadLine();
        _process.BeginErrorReadLine();
    }

    private void HandleOutput(object sender, DataReceivedEventArgs e)
    {
        Dispatcher.UIThread.InvokeAsync(() => OutputTextBox.Text += e.Data + Environment.NewLine);
    }

    private void AssignEvents()
    {
        InputTextBox.KeyDown += (sender, e) =>
        {
            if (e.Key == Key.Enter)
            {
                var text = InputTextBox.Text;
                InputTextBox.Text = string.Empty;

                _process.StandardInput.WriteLine(text);
                _process.StandardInput.Flush();
            }
        };

        Closing += (sender, e) =>
        {
            try
            {
                _process.StandardInput.Close();

                if (!_process.WaitForExit(1000))
                {
                    _process.Kill(entireProcessTree: true);
                }

                _process.CancelOutputRead();
                _process.CancelErrorRead();
                _process.Dispose();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while closing terminal process");
            }
        };
    }
}