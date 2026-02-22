using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using SkEditor.API;

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
        string? host = HostTextBox.Text;
        int? port = int.TryParse(PortTextBox.Text, out int p) ? p : null;
        string? username = UsernameTextBox.Text;
        string? password = PasswordTextBox.Text;
        
        if (string.IsNullOrEmpty(host) || port == null || string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            await SkEditorAPI.Windows.ShowMessage("Error", "Please fill in all fields.");
            return;
        }

        using var client = new SftpClient(host, port.Value, username, password);

        try
        {
            using var cts = new CancellationTokenSource();
            
            SkEditorAPI.Logs.Info("Attempting to connect to SSH server...");
            Stopwatch sw = Stopwatch.StartNew();
            await client.ConnectAsync(cts.Token);
            sw.Stop();
            await SkEditorAPI.Windows.ShowMessage("Connection Successful", $"Successfully connected to {host}:{port} as {username} in {sw.ElapsedMilliseconds} ms.");
            
            await foreach (ISftpFile file in client.ListDirectoryAsync("/", CancellationToken.None))
            {
                SkEditorAPI.Logs.Info($"Found file: {file.FullName} (Size: {file.Length} bytes), Permissions: {file.Attributes} at time: {DateTime.Now}");
                
                if (file.Name.EndsWith(".txt") || file.Name.EndsWith(".yml"))
                {
                    Stopwatch sw2 = Stopwatch.StartNew(); 
                    await using var stream = client.OpenRead(file.FullName);
                    using var reader = new System.IO.StreamReader(stream);
                    string content = await reader.ReadToEndAsync(cts.Token);
                    sw2.Stop();
                    SkEditorAPI.Logs.Info($"Read file {file.FullName} in {sw2.ElapsedMilliseconds} ms, content length: {content.Length} characters");
                }
            }
        }
        catch (Exception ex)
        {
            await SkEditorAPI.Windows.ShowMessage("Connection Failed", $"Error: {ex.Message}");
        }
    }
}