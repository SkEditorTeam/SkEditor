using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
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
            "Used as documentation provider and script host (via skUnity Parser)", "skUnity.svg");
        SetupEntry("SkriptHub", "https://skripthub.net/dashboard/api/", "SkriptHubAPIKey",
            "Used as documentation provider", "SkriptHub.svg");
        SetupEntry("SkriptMC", "https://skript-mc.fr/developer/", "SkriptMCAPIKey",
            "Used as documentation provider");
        SetupEntry("skript.pl", "https://code.skript.pl/api-key", "CodeSkriptPlApiKey",
            "Used as script host", "skriptpl.svg");
        SetupEntry("Pastebin", "https://pastebin.com/doc_api", "PastebinApiKey",
            "Used as script host", "Pastebin.svg");
    }

    public void SetupEntry(string name, string url, string key, string description, string? icon = null)
    {
        icon ??= name + ".png";
        ElementsPanel.Children.Add(new ConnectionEntryControl(name, icon, url, key, description));
    }
}