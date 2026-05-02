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
            .WithIcon("DiscordIcon")
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
                CompoundSetting
                    .Create(TranslatableString.FromRawString("skUnity"))
                    .WithDescription(
                        TranslatableString.FromRawString(
                            "Used as a documentation provider and a script host (via skUnity Parser)"))
                    .WithIcon("AppIcon")
                    .WithVerticalLayout()
                    .WithChildren(
                        ActionSetting
                            .Create(TranslationKey.SettingsConnectionsDashboardButton)
                            .WithAction(TranslationKey.SettingsConnectionsDashboardButton,
                                () => LinkOpener.OpenLink("https://skeditor.dev")),
                        TextSetting
                            .Create(TranslatableString.FromRawString("API Key"))
                            .WithProperty(secrets.GetProperty<string>(SecretsConfigProperty.SkUnityApiKey))
                            .WithPlaceholder(TranslatableString.FromRawString("Your skUnity API key"))
                            .WithPassword()
                            .WithWidth(200)
                    )
            ]);
    }
}