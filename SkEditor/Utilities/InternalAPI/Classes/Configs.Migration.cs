using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Styling;

namespace SkEditor.Utilities.InternalAPI.Classes;

public partial class Configs
{
    public async Task MigrateConfigsAsync()
    {
        string oldSettings = Path.Combine(GetAppDataPath(), "settings.json");
        if (!File.Exists(oldSettings))
        {
            return;
        }

        string json = await File.ReadAllTextAsync(oldSettings);

        OldAppConfig? oldAppConfig =
            JsonSerializer.Deserialize<OldAppConfig>(json, OldAppJsonContext.Default.OldAppConfig);

        if (oldAppConfig != null)
        {
            AppSettings appSettings = GetAppSettings();
            appSettings.CheckForAppUpdates = oldAppConfig.CheckForUpdates;
            appSettings.CheckForExternalFileChanges = oldAppConfig.CheckForChanges;

            string oldCurrentTheme = oldAppConfig.CurrentTheme;
            Theme? theme = ThemeEditor.Themes.FirstOrDefault(x =>
                x.Name.Equals(oldCurrentTheme) || x.FileName.Equals(oldCurrentTheme));
            appSettings.CurrentTheme = theme == null ? "Default" : theme.Name;

            appSettings.CustomUiScale = oldAppConfig.CustomUiScale;
            appSettings.FileSyntaxes = oldAppConfig.FileSyntaxes;
            appSettings.EditorFontFamily = oldAppConfig.Font == "Default"
                ? null
                : oldAppConfig.Font;
            appSettings.ForceNativeTitleBar = oldAppConfig.ForceNativeTitleBar;
            appSettings.HighlightCurrentLine = oldAppConfig.HighlightCurrentLine;
            appSettings.IsAutoIndentEnabled = oldAppConfig.IsAutoIndentEnabled;
            appSettings.IsAutoPairingEnabled = oldAppConfig.IsAutoPairingEnabled;
            appSettings.IsAutoSaveEnabled = oldAppConfig.IsAutoSaveEnabled;
            appSettings.IsDevModeEnabled = oldAppConfig.IsDevModeEnabled;
            appSettings.IsDiscordRichPresenceEnabled = oldAppConfig.IsDiscordRpcEnabled;
            appSettings.IsPasteIndentationEnabled = oldAppConfig.IsPasteIndentationEnabled;
            appSettings.IsProjectSingleClickEnabled = oldAppConfig.IsProjectSingleClickEnabled;
            appSettings.IsSidebarAnimationEnabled = oldAppConfig.IsSidebarAnimationEnabled;
            appSettings.IsWordWrapEnabled = oldAppConfig.IsWrappingEnabled;
            appSettings.IsEditorZoomLevelSyncEnabled = oldAppConfig.IsZoomSyncEnabled;
            appSettings.IsSidebarWidthSyncEnabled = oldAppConfig.IsSidebarWidthSyncEnabled;
            appSettings.Language = oldAppConfig.Language;
            appSettings.EditorTabSize = oldAppConfig.TabSize;
            appSettings.UseEditorTabsForIndentation = !oldAppConfig.UseSpacesInsteadOfTabs;

            StateConfig stateConfig = GetStateConfig();
            stateConfig.LastUsedPublishService = oldAppConfig.LastUsedPublishService;
            stateConfig.GuiGeneratorSkriptGuiAddonUsed = oldAppConfig.UseSkriptGui;
            stateConfig.SidebarPanelSizes = oldAppConfig.SidebarPanelSizes;
            stateConfig.FileTypeChoices = oldAppConfig.FileTypeChoices;

            ExperimentsConfig experimentsConfig = GetExperimentsConfig();
            experimentsConfig.AutoCompletionPrototypeEnabled = oldAppConfig.EnableAutoCompletionExperiment;
            experimentsConfig.ProjectsEnabled = oldAppConfig.EnableProjectsExperiment;
            experimentsConfig.HexPreviewEnabled = oldAppConfig.EnableHexPreview;
            experimentsConfig.CodeParserEnabled = oldAppConfig.EnableCodeParser;
            experimentsConfig.FoldingEnabled = oldAppConfig.EnableFolding;
            experimentsConfig.SessionRestoringEnabled = oldAppConfig.EnableSessionRestoring;
            experimentsConfig.RealtimeCodeParserEnabled = oldAppConfig.EnableRealtimeCodeParser;

            SecretsConfig secretsConfig = GetSecretsConfig();
            secretsConfig.SkUnityApiKey = oldAppConfig.SkUnityApiKey;
            secretsConfig.SkriptMcApiKey = oldAppConfig.SkriptMcApiKey;
            secretsConfig.PastebinApiKey = oldAppConfig.PastebinApiKey;
            secretsConfig.CodeSkriptPlApiKey = oldAppConfig.CodeSkriptPlApiKey;

            await SaveAllConfigsAsync();
            File.Delete(oldSettings);
        }
    }
}