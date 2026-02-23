using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using Renci.SshNet;

namespace SkEditor.Views.Windows;

public partial class RemoteConnectionWindow : AppWindow
{
    public RemoteConnectionWindow()
    {
        InitializeComponent();
        Focusable = true;

        TestConnectionButton.Command = new AsyncRelayCommand(TestConnection);
    }

    private async Task TestConnection()
    {
        InfoBar.IsOpen = false;
        
        string? host = HostTextBox.Text;
        int? port = int.TryParse(PortTextBox.Text, out int p) ? p : null;
        string? username = UsernameTextBox.Text;
        string? password = PasswordTextBox.Text;
        
        if (string.IsNullOrEmpty(host) || port == null || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            // ShowMessage("Please fill in all fields.", InfoBarSeverity.Error);
            return;
        }

        using var client = new SftpClient(host, port.Value, username, password);

        try
        {
            using var cts = new CancellationTokenSource();
            
            Stopwatch sw = Stopwatch.StartNew();
            await client.ConnectAsync(cts.Token);
            sw.Stop();
            
            if (client.IsConnected)
            {
                ShowMessage($"Successfully connected to {host}:{port} in {sw.ElapsedMilliseconds} ms.", InfoBarSeverity.Success);
            }
            else
            {
                ShowMessage("Unable to connect to the server.", InfoBarSeverity.Error);
            }
        }
        catch (Exception ex)
        {
            ShowMessage($"Error: {ex.Message}", InfoBarSeverity.Error);
        }
    }

    private void ShowMessage(string message, InfoBarSeverity severity)
    {
        InfoBar.Severity = severity;
        InfoBar.Message = message;
        InfoBar.IsOpen = true;
    }
}