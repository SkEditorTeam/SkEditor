using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;

namespace SkEditor.Views.Settings;

public partial class GeneralPage : UserControl
{
    public GeneralPage()
    {
        InitializeComponent();

        DataContext = SkEditorAPI.Core.GetAppConfig();

        AssignCommands();
        LoadLanguages();
        LoadIndentation();
    }

    private void LoadLanguages()
    {
        string[] files = Directory.GetFiles(Translation.LanguagesFolder);
        foreach (string file in files)
        {
            LanguageComboBox.Items.Add(Path.GetFileNameWithoutExtension(file));
        }

        LanguageComboBox.SelectedItem = SkEditorAPI.Core.GetAppConfig().Language;
        LanguageComboBox.SelectionChanged += (_, _) =>
        {
            string? language = LanguageComboBox.SelectedItem.ToString();
            if (string.IsNullOrEmpty(language)) return;
            
            SkEditorAPI.Core.GetAppConfig().Language = language;
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
        AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();
        string tag = appConfig.UseSpacesInsteadOfTabs ? "spaces" : "tabs";
        int amount = appConfig.TabSize;

        foreach (object? item in IndentationTypeComboBox.Items)
        {
            if ((item as ComboBoxItem)?.Tag?.ToString() != tag) continue;

            IndentationTypeComboBox.SelectedItem = item;
            break;
        }

        foreach (object? item in IndentationAmountComboBox.Items)
        {
            if ((item as ComboBoxItem)?.Tag?.ToString() != amount.ToString()) continue;

            IndentationAmountComboBox.SelectedItem = item;
            break;
        }
    }

    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));
        RpcToggleSwitch.Command = new RelayCommand(ToggleRpc);
        WrappingToggleSwitch.Command = new RelayCommand(ToggleWrapping);
        ZoomSyncToggleSwitch.Command = new RelayCommand(ToggleZoomSync);
        ProjectSingleClickToggleSwitch.Command = new RelayCommand(() => ToggleSetting("IsProjectSingleClickEnabled"));

        IndentationAmountComboBox.SelectionChanged += (_, _) =>
        {
            AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();
            bool success = int.TryParse((IndentationAmountComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString(), out int result);
            if (!success)
            {
                return;
            }

            appConfig.TabSize = result;
            SkEditorAPI.Files.GetOpenedEditors()
                .ForEach(file => file.Editor!.Options.IndentationSize = appConfig.TabSize);
        };
        IndentationTypeComboBox.SelectionChanged += (_, _) =>
        {
            AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();
            appConfig.UseSpacesInsteadOfTabs =
                (IndentationTypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "spaces";
            SkEditorAPI.Files.GetOpenedEditors().ForEach(file =>
                file.Editor!.Options.ConvertTabsToSpaces = appConfig.UseSpacesInsteadOfTabs);
        };
    }

    private void ToggleRpc()
    {
        ToggleSetting("IsDiscordRpcEnabled");

        if (SkEditorAPI.Core.GetAppConfig().IsDiscordRpcEnabled)
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

        textEditors.ForEach(textEditor => textEditor.WordWrap = SkEditorAPI.Core.GetAppConfig().IsWrappingEnabled);
    }

    private void ToggleZoomSync()
    {
        ToggleSetting("IsZoomSyncEnabled");

        if (!SkEditorAPI.Core.GetAppConfig().IsZoomSyncEnabled)
        {
            return;
        }

        List<TextEditor> textEditors = SkEditorAPI.Files.GetOpenedEditors().Select(e => e.Editor).ToList()!;
        if (textEditors.Count == 0) return;

        double fontSize = textEditors.First().FontSize;
        textEditors.ForEach(textEditor => { textEditor.FontSize = fontSize; });
    }

    private static void ToggleSetting(string propertyName)
    {
        AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();
        PropertyInfo? property = appConfig.GetType().GetProperty(propertyName);

        if (property == null || property.PropertyType != typeof(bool))
        {
            return;
        }

        bool? currentValue = (bool?)property.GetValue(appConfig);
        property.SetValue(appConfig, !currentValue);
    }
}