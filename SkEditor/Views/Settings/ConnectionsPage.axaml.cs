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
            "Used as a documentation provider and a script host (via skUnity Parser)", "skUnity.svg");
        SetupEntry("SkriptMC", "https://skript-mc.fr/developer/", "SkriptMCAPIKey",
            "Used as a documentation provider");
        SetupEntry("skript.pl", "https://code.skript.pl/api-key", "CodeSkriptPlApiKey",
            "Used as a script host", "skriptpl.svg");
        SetupEntry("Pastebin", "https://pastebin.com/doc_api", "PastebinApiKey",
            "Used as a script host", "Pastebin.svg");
    }

    public void SetupEntry(string name, string url, string key, string description, string? icon = null)
    {
        icon ??= name + ".png";
        ElementsPanel.Children.Add(new ConnectionEntryControl(name, icon, url, key, description));
    }
}