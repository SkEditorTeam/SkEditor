using System.Text.RegularExpressions;
using DiscordRPC;
using DiscordRPC.Entities;
using SkEditor.Configuration.Configs;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Integrations.Discord;

public partial class DiscordIntegrationService(
    AppSettings appSettings,
    ITabService tabs)
    : IDiscordIntegrationService
{
    private const string ApplicationId = "1152625623777431662";
    private DiscordRpcClient? _client;

    public void SetUp()
    {
        appSettings.PropertyChanged += (_, e) =>
        {
            switch (e.PropertyName)
            {
                case nameof(AppSettings.IsDiscordRichPresenceEnabled) when appSettings.IsDiscordRichPresenceEnabled:
                    Initialize();
                    break;
                case nameof(AppSettings.IsDiscordRichPresenceEnabled):
                    Uninitialize();
                    break;
                case nameof(AppSettings.IsDiscordRichPresenceDetailsEnabled)
                    when appSettings.IsDiscordRichPresenceDetailsEnabled:
                    UpdateDetails(tabs.SelectedTab);
                    break;
                case nameof(AppSettings.IsDiscordRichPresenceDetailsEnabled):
                    _client?.UpdateDetails(string.Empty);
                    break;
            }
        };

        if (appSettings.IsDiscordRichPresenceEnabled)
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        if (!appSettings.IsDiscordRichPresenceEnabled)
        {
            return;
        }

        _client = new DiscordRpcClient(ApplicationId);
        _client.Initialize();

        _client.SetPresence(new RichPresence
        {
            Timestamps = Timestamps.Now,
            Assets = new DiscordRPC.Entities.Assets
            {
                LargeImageKey = "image_large",
                LargeImageText = "SkEditor"
            }
        });

        tabs.SelectedTabChanged += tab =>
        {
            if (!appSettings.IsDiscordRichPresenceEnabled || !appSettings.IsDiscordRichPresenceDetailsEnabled)
            {
                return;
            }

            UpdateDetails(tab);
        };
    }

    public void Uninitialize()
    {
        if (_client == null)
        {
            return;
        }

        _client.ClearPresence();
        _client.Dispose();
    }

    private void UpdateDetails(ITab? tab)
    {
        if (_client?.IsInitialized != true)
        {
            return;
        }

        if (tab is not FileTabViewModel fileTab)
        {
            _client.UpdateDetails("Idle");
            return;
        }

        string tabName = fileTab.Header.Value;
        tabName = UnicodeControlCharacterFilter().Replace(tabName, string.Empty);
        string translation = $"Editing: {tabName}";
        if (translation.Length > 128)
        {
            translation = translation[..125] + "...";
        }

        _client.UpdateDetails(translation);
    }

    [GeneratedRegex(@"[^\P{C}\n]+")]
    private static partial Regex UnicodeControlCharacterFilter();
}