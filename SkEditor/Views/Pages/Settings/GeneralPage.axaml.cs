using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
using AppSettings = SkEditor.Utilities.Settings.Configs.AppSettings;
using DiscordRpcUpdater = SkEditor.Utilities.Discord.DiscordRpcUpdater;

namespace SkEditor.Views.Pages.Settings;

public partial class GeneralPage : UserControl
{
    public List<string> AvailableLanguages { get; } = [];
    
    public GeneralPage()
    {
        InitializeComponent();

        AssignCommands();
        LoadLanguages();
        LoadIndentation();
    }

    private void LoadLanguages()
    {
        string[] files = Directory.GetFiles(Translation.LanguagesFolder);
        foreach (string file in files)
        {
            AvailableLanguages.Add(Path.GetFileNameWithoutExtension(file));
        }
        
        AvailableLanguages.Add("English");
        AvailableLanguages.Sort();
        
        LanguageComboBox.ItemsSource = AvailableLanguages;

        LanguageComboBox.SelectedItem = SkEditorAPI.Configs.GetAppSettings().Language;
        LanguageComboBox.SelectionChanged += (_, _) =>
        {
            string? language = LanguageComboBox.SelectedItem.ToString();
            if (string.IsNullOrEmpty(language))
            {
                return;
            }

            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Translation.ChangeLanguage(language);

                // Regenerate the text editor context menu
                if (!SkEditorAPI.Files.IsEditorOpen())
                {
                    return;
                }

                foreach (OpenedFile openedFile in SkEditorAPI.Files.GetOpenedEditors())
                {
                    openedFile.Editor!.ContextFlyout = FileBuilder.GetContextMenu(openedFile.Editor);
                }
            });
        };
    }

    private void LoadIndentation()
    {
        AppSettings appSettings = SkEditorAPI.Configs.GetAppSettings();
        string tag = appSettings.UseEditorTabsForIndentation ? "tabs" : "spaces";
        int amount = appSettings.EditorTabSize;

        foreach (object? item in IndentationTypeComboBox.Items)
        {
            if ((item as ComboBoxItem)?.Tag?.ToString() != tag)
            {
                continue;
            }

            IndentationTypeComboBox.SelectedItem = item;
            break;
        }

        foreach (object? item in IndentationAmountComboBox.Items)
        {
            if ((item as ComboBoxItem)?.Tag?.ToString() != amount.ToString())
            {
                continue;
            }

            IndentationAmountComboBox.SelectedItem = item;
            break;
        }
    }

    private void AssignCommands()
    {
        RpcToggleSwitch.Command = new RelayCommand(ToggleRpc);
        WrappingToggleSwitch.Command = new RelayCommand(ToggleWrapping);
        ZoomSyncToggleSwitch.Command = new RelayCommand(ToggleZoomSync);
        ProjectSingleClickToggleSwitch.Command = new RelayCommand(() => ToggleSetting("IsProjectSingleClickEnabled"));

        IndentationAmountComboBox.SelectionChanged += (_, _) =>
        {
            AppSettings oldAppConfig = SkEditorAPI.Configs.GetAppSettings();
            bool success = int.TryParse((IndentationAmountComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString(),
                out int result);
            if (!success)
            {
                return;
            }

            oldAppConfig.EditorTabSize = result;
            SkEditorAPI.Files.GetOpenedEditors()
                .ForEach(file => file.Editor!.Options.IndentationSize = oldAppConfig.EditorTabSize);
        };
        IndentationTypeComboBox.SelectionChanged += (_, _) =>
        {
            AppSettings oldAppConfig = SkEditorAPI.Configs.GetAppSettings();
            oldAppConfig.UseEditorTabsForIndentation =
                (IndentationTypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "tabs";
            SkEditorAPI.Files.GetOpenedEditors().ForEach(file =>
                file.Editor!.Options.ConvertTabsToSpaces = !oldAppConfig.UseEditorTabsForIndentation);
        };
    }

    private void ToggleRpc()
    {
        ToggleSetting("IsDiscordRpcEnabled");

        if (SkEditorAPI.Configs.GetAppSettings().IsDiscordRichPresenceEnabled)
        {
            DiscordRpcUpdater.Initialize();
        }
        else
        {
            DiscordRpcUpdater.Uninitialize();
        }
    }

    private void ToggleWrapping()
    {
        ToggleSetting("IsWrappingEnabled");

        List<TextEditor> textEditors = SkEditorAPI.Files.GetOpenedEditors().Select(e => e.Editor).ToList()!;

        textEditors.ForEach(textEditor => textEditor.WordWrap = SkEditorAPI.Configs.GetAppSettings().IsWordWrapEnabled);
    }

    private void ToggleZoomSync()
    {
        ToggleSetting("IsZoomSyncEnabled");

        if (!SkEditorAPI.Configs.GetAppSettings().IsEditorZoomLevelSyncEnabled)
        {
            return;
        }

        List<TextEditor> textEditors = SkEditorAPI.Files.GetOpenedEditors().Select(e => e.Editor).ToList()!;
        if (textEditors.Count == 0)
        {
            return;
        }

        double fontSize = textEditors.First().FontSize;
        textEditors.ForEach(textEditor => { textEditor.FontSize = fontSize; });
    }

    private static void ToggleSetting(string propertyName)
    {
        AppSettings appSettings = SkEditorAPI.Configs.GetAppSettings();
        PropertyInfo? property = appSettings.GetType().GetProperty(propertyName);

        if (property == null || property.PropertyType != typeof(bool))
        {
            return;
        }

        bool? currentValue = (bool?)property.GetValue(appSettings);
        property.SetValue(appSettings, !currentValue);
    }
}