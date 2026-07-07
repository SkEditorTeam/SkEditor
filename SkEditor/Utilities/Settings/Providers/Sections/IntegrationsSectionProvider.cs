using System;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Models.Settings;
using SkEditor.Models.Settings.Types;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using AppSettingsProperty = SkEditor.Utilities.Settings.Configs.AppSettings.AppSettingsProperty;
using SecretsConfigProperty = SkEditor.Utilities.Settings.Configs.SecretsConfig.SecretsConfigProperty;

namespace SkEditor.Utilities.Settings.Providers.Sections;

public class IntegrationsSectionProvider(
    AppSettings appSettings,
    SecretsConfig secrets,
    ILinkService links) : ISettingsSectionProvider
{
    public SettingSection GetSection()
    {
        SettingSection integrations = SettingSection.Create("Integrations", TranslationKey.SettingsSectionIntegrations)
            .WithIcon(Symbol.PlugConnected);

        SettingSection integrationsDiscord = CreateDiscordSection();
        SettingSection integrationsApiKeys = CreateApiKeysSection();

        integrations.WithSubsections([integrationsDiscord, integrationsApiKeys]);

        return integrations;
    }

    private SettingSection CreateDiscordSection()
    {
        return SettingSection.Create("Discord", TranslationKey.SettingsIntegrationsSubsectionDiscord)
            .WithIcon("DiscordIconOutline")
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsIntegrationsDiscordRichPresence)
                    .WithDescription(TranslationKey.SettingsIntegrationsDiscordRichPresenceDescription)
                    .WithIcon(Symbol.Games)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsDiscordRichPresenceEnabled)),

                ToggleSetting
                    .Create(TranslationKey.SettingsIntegrationsDiscordRichPresenceShowDetails)
                    .WithDescription(TranslationKey.SettingsIntegrationsDiscordRichPresenceShowDetailsDescription)
                    .WithIcon(Symbol.TextBulletListSquare)
                    .WithProperty(
                        appSettings.GetProperty<bool>(AppSettingsProperty.IsDiscordRichPresenceDetailsEnabled))
            ]);
    }

    private SettingSection CreateApiKeysSection()
    {
        return SettingSection.Create("API Keys", TranslationKey.SettingsIntegrationsSubsectionApiKeys)
            .WithIcon(Symbol.Key)
            .WithSettings(
            [
                CreateApiKeySetting(
                    TranslationKey.SettingsIntegrationsApiKeysSkUnity,
                    TranslationKey.SettingsIntegrationsApiKeysSkUnityDescription,
                    "skUnity",
                    "skUnityIcon",
                    "https://skunity.com/dashboard/skunity-api",
                    secrets.GetProperty<string>(SecretsConfigProperty.SkUnityApiKey)),

                CreateApiKeySetting(
                    TranslationKey.SettingsIntegrationsApiKeysPastebin,
                    TranslationKey.SettingsIntegrationsApiKeysPastebinDescription,
                    "Pastebin",
                    "PastebinIcon",
                    "https://pastebin.com/doc_api",
                    secrets.GetProperty<string>(SecretsConfigProperty.PastebinApiKey))
            ]);
    }

    private ExpandableSetting CreateApiKeySetting(
        TranslationKey nameKey,
        TranslationKey descriptionKey,
        string serviceName,
        string icon,
        string link,
        (Func<string> Get, Action<string> Set) property)
    {
        return ExpandableSetting
            .Create(nameKey)
            .WithDescription(descriptionKey)
            .WithIcon(icon)
            .WithChildren(
                ActionSetting
                    .Create(TranslationKey.SettingsIntegrationsApiKeysDashboard)
                    .WithDescription(TranslationKey.SettingsIntegrationsApiKeysDashboardDescription)
                    .WithAction(TranslationKey.SettingsIntegrationsApiKeysDashboardButton,
                        () => links.OpenLink(link)),
                TextSetting
                    .Create(TranslationKey.SettingsIntegrationsApiKeysApiKey)
                    .WithProperty(property)
                    .WithPlaceholder(new TranslatableString(
                        TranslationKey.SettingsIntegrationsApiKeysApiKeyPlaceholder,
                        serviceName))
                    .WithPassword()
                    .WithWidth(200)
            );
    }
}