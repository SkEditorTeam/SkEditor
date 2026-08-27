using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Services;

public class LegacyConfigMigrator(IConfigService config)
{
    /// <summary>
    ///     Check and migrate old configuration files to the new format asynchronously.
    /// </summary>
    /// <returns>>A task representing the asynchronous operation.</returns>
    public async Task MigrateConfigsAsync()
    {
        string newPath = config.GetAppDataPath();
        string? oldPath = Directory.GetParent(newPath)?.FullName;

        if (string.IsNullOrEmpty(oldPath))
        {
            return;
        }

        string oldSettings = Path.Combine(oldPath, "settings.json");

        if (!Directory.Exists(oldPath) || Directory.Exists(newPath) || !File.Exists(oldSettings))
        {
            return;
        }

        string json = await File.ReadAllTextAsync(oldSettings);

        OldAppConfig? oldAppConfig =
            JsonSerializer.Deserialize<OldAppConfig>(json, OldAppJsonContext.Default.OldAppConfig);

        AppSettings? appSettings = config.GetConfig<AppSettings>();
        StateConfig? stateConfig = config.GetConfig<StateConfig>();
        ExperimentsConfig? experimentsConfig = config.GetConfig<ExperimentsConfig>();
        SecretsConfig? secretsConfig = config.GetConfig<SecretsConfig>();

        if (appSettings == null || stateConfig == null || experimentsConfig == null || secretsConfig == null)
        {
            return;
        }

        if (oldAppConfig != null)
        {
            appSettings.CurrentTheme = oldAppConfig.CurrentTheme;
            appSettings.CustomUiScale = oldAppConfig.CustomUiScale;
            appSettings.EditorFontFamily = oldAppConfig.Font == "Default"
                ? null
                : oldAppConfig.Font;
            appSettings.HighlightCurrentLine = oldAppConfig.HighlightCurrentLine;
            appSettings.IsAutoIndentEnabled = oldAppConfig.IsAutoIndentEnabled;
            appSettings.IsAutoPairingEnabled = oldAppConfig.IsAutoPairingEnabled;
            appSettings.IsAutoSaveEnabled = oldAppConfig.IsAutoSaveEnabled;
            appSettings.IsDevModeEnabled = oldAppConfig.IsDevModeEnabled;
            appSettings.IsDiscordRichPresenceEnabled = oldAppConfig.IsDiscordRpcEnabled;
            appSettings.IsExplorerSingleClickOpenEnabled = oldAppConfig.IsProjectSingleClickEnabled;
            appSettings.IsWordWrapEnabled = oldAppConfig.IsWrappingEnabled;
            appSettings.IsEditorZoomLevelSyncEnabled = oldAppConfig.IsZoomSyncEnabled;
            appSettings.Language = oldAppConfig.Language;
            appSettings.EditorTabSize = oldAppConfig.TabSize;
            appSettings.UseEditorTabsForIndentation = !oldAppConfig.UseSpacesInsteadOfTabs;

            stateConfig.LastUsedPublishService = oldAppConfig.LastUsedPublishService;
            stateConfig.GuiGeneratorSkriptGuiAddonUsed = oldAppConfig.UseSkriptGui;

            experimentsConfig.ColorPreviewEnabled = oldAppConfig.EnableHexPreview;

            secretsConfig.SkUnityApiKey = oldAppConfig.SkUnityApiKey;
            secretsConfig.PastebinApiKey = oldAppConfig.PastebinApiKey;

            await config.SaveAllConfigsAsync();
        }
    }
}