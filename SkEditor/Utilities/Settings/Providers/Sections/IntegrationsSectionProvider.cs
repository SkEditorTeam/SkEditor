using System;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Models.Types;
using AppSettingsProperty = SkEditor.Utilities.Settings.Configs.AppSettings.AppSettingsProperty;
using SecretsConfigProperty = SkEditor.Utilities.Settings.Configs.SecretsConfig.SecretsConfigProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class IntegrationsSectionProvider(
    AppSettings appSettings,
    SecretsConfig secrets) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        SettingSection integrations = SettingSection.Create(TranslatableString.FromRawString("Integrations"))
            .WithIcon(Symbol.PlugConnected);

        SettingSection integrationsDiscord = CreateDiscordSection();
        SettingSection integrationsApiKeys = CreateApiKeysSection();

        integrations.WithSubsections([integrationsDiscord, integrationsApiKeys]);

        return integrations;
    }

    private SettingSection CreateDiscordSection()
    {
        return SettingSection.Create(TranslatableString.FromRawString("Discord"))
            .WithIcon("DiscordIconOutline")
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsGeneralRpc)
                    .WithDescription(TranslationKey.SettingsGeneralRpcDescription)
                    .WithIcon(Symbol.Games)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsDiscordRichPresenceEnabled))
            ]);
    }

    private SettingSection CreateApiKeysSection()
    {
        return SettingSection.Create(TranslatableString.FromRawString("API Keys"))
            .WithIcon(Symbol.Key)
            .WithSettings(
            [
                CreateApiKeySetting(
                    "skUnity",
                    "Used as a documentation provider and a script host (via skUnity Parser)",
                    "skUnityIcon",
                    "https://skeditor.dev",
                    secrets.GetProperty<string>(SecretsConfigProperty.SkUnityApiKey))
            ]);
    }

    private static ExpandableSetting CreateApiKeySetting(string name, string description, string icon, string link,
        (Func<string> Get, Action<string> Set) property)
    {
        return ExpandableSetting
            .Create(TranslatableString.FromRawString(name))
            .WithDescription(
                TranslatableString.FromRawString(description))
            .WithIcon(icon)
            .WithChildren(
                ActionSetting
                    .Create(TranslatableString.FromRawString("Dashboard"))
                    .WithAction(TranslationKey.SettingsConnectionsDashboardButton, () => LinkOpener.OpenLink(link)),
                TextSetting
                    .Create(TranslatableString.FromRawString("API Key"))
                    .WithProperty(property)
                    .WithPlaceholder(TranslatableString.FromRawString($"Your {name} API key"))
                    .WithPassword()
                    .WithWidth(200)
            );
    }
}