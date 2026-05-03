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
        SettingSection integrations = SettingSection.Create(TranslationKey.SettingsSectionIntegrations)
            .WithIcon(Symbol.PlugConnected);

        SettingSection integrationsDiscord = CreateDiscordSection();
        SettingSection integrationsApiKeys = CreateApiKeysSection();

        integrations.WithSubsections([integrationsDiscord, integrationsApiKeys]);

        return integrations;
    }

    private SettingSection CreateDiscordSection()
    {
        return SettingSection.Create(TranslationKey.SettingsIntegrationsSubsectionDiscord)
            .WithIcon("DiscordIconOutline")
            .WithSettings(
            [
                ToggleSetting
                    .Create(TranslationKey.SettingsIntegrationsDiscordRichPresence)
                    .WithDescription(TranslationKey.SettingsIntegrationsDiscordRichPresenceDescription)
                    .WithIcon(Symbol.Games)
                    .WithProperty(appSettings.GetProperty<bool>(AppSettingsProperty.IsDiscordRichPresenceEnabled))
            ]);
    }

    private SettingSection CreateApiKeysSection()
    {
        return SettingSection.Create(TranslationKey.SettingsIntegrationsSubsectionApiKeys)
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

    private static ExpandableSetting CreateApiKeySetting(
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
                        () => LinkOpener.OpenLink(link)),
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