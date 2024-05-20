using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;

namespace SkEditor.Views.Settings;

public partial class ConnectionsPage : UserControl
{
    public ConnectionsPage()
    {
        InitializeComponent();
        
        AssignCommands();
    }

    public void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));
        
        SetupEntry(SkUnityEntry, "https://skunity.com/dashboard/skunity-api", "SkUnityAPIKey");
        SetupEntry(SkriptHubEntry, "https://skripthub.net/dashboard/api/", "SkriptHubAPIKey");
        SetupEntry(SkriptMCEntry, "https://skript-mc.fr/developer/", "SkriptMCAPIKey");
    }
    
    public void SetupEntry(StackPanel entry, string url, string key)
    {
        var button = entry.Children[0] as Button;
        var textBox = entry.Children[1] as TextBox;
        
        button.Click += async (sender, e) =>
        {
            Process.Start(new ProcessStartInfo(url)
            {
                UseShellExecute = true
            });
        };
        
        textBox.Text = ApiVault.Get().GetAppConfig().GetOptionValue<string>(key);
        textBox.TextChanged += (_, _) =>
        {
            ApiVault.Get().GetAppConfig().SetOptionValue(key, textBox.Text);
        };
    }
}