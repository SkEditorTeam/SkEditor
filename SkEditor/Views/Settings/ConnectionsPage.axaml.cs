using System.Diagnostics;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Controls;

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
        
        SetupEntry("skUnity", "https://skunity.com/dashboard/skunity-api", "SkUnityAPIKey",
            "Used as documentation provider and script host (via skUnity Parser)");
        SetupEntry("SkriptHub", "https://skripthub.net/dashboard/api/", "SkriptHubAPIKey",
            "Used as documentation provider");
        SetupEntry("SkriptMC", "https://skript-mc.fr/developer/", "SkriptMCAPIKey",
            "Used as documentation provider");
        SetupEntry("SkriptPL", "https://code.skript.pl/api-key", "CodeSkriptPlApiKey",
            "Used as script host");
        SetupEntry("Pastebin", "https://pastebin.com/doc_api", "PastebinApiKey",
            "Used as script host");
    }
    
    public void SetupEntry(string name, string url, string key, string description)
    {
        ElementsPanel.Children.Add(new ConnectionEntryControl(name, url, key, description));
    }
}