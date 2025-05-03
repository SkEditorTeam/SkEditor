using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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
            string language = LanguageComboBox.SelectedItem.ToString();
            SkEditorAPI.Core.GetAppConfig().Language = language;
            Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Translation.ChangeLanguage(language);

                // Regenerate the text editor context menu
                // TODO: Context menu language doesn't change, when user has documentation tab opened.
                if (!SkEditorAPI.Files.IsEditorOpen())
                    return;

                TextEditor editor = SkEditorAPI.Files.GetCurrentOpenedFile().Editor;
                editor.ContextFlyout = FileBuilder.GetContextMenu(editor);
            });
        };
    }

    private void LoadIndentation()
    {
        var appConfig = SkEditorAPI.Core.GetAppConfig();
        var tag = appConfig.UseSpacesInsteadOfTabs ? "spaces" : "tabs";
        var amount = appConfig.TabSize;

        foreach (var item in IndentationTypeComboBox.Items)
        {
            if ((item as ComboBoxItem).Tag.ToString() == tag)
            {
                IndentationTypeComboBox.SelectedItem = item;
                break;
            }
        }

        foreach (var item in IndentationAmountComboBox.Items)
        {
            if ((item as ComboBoxItem).Tag.ToString() == amount.ToString())
            {
                IndentationAmountComboBox.SelectedItem = item;
                break;
            }
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
            var appConfig = SkEditorAPI.Core.GetAppConfig();
            appConfig.TabSize = int.Parse((IndentationAmountComboBox.SelectedItem as ComboBoxItem).Tag.ToString());
            SkEditorAPI.Files.GetOpenedEditors().ForEach(file => file.Editor.Options.IndentationSize = appConfig.TabSize);
        };
        IndentationTypeComboBox.SelectionChanged += (_, _) =>
        {
            var appConfig = SkEditorAPI.Core.GetAppConfig();
            appConfig.UseSpacesInsteadOfTabs = (IndentationTypeComboBox.SelectedItem as ComboBoxItem).Tag.ToString() == "spaces";
            SkEditorAPI.Files.GetOpenedEditors().ForEach(file => file.Editor.Options.ConvertTabsToSpaces = appConfig.UseSpacesInsteadOfTabs);
        };
    }

    private void ToggleRpc()
    {
        ToggleSetting("IsDiscordRpcEnabled");

        if (SkEditorAPI.Core.GetAppConfig().IsDiscordRpcEnabled) DiscordRpcUpdater.Initialize();
        else DiscordRpcUpdater.Uninitialize();
    }

    private void ToggleWrapping()
    {
        ToggleSetting("IsWrappingEnabled");

        List<TextEditor> textEditors = SkEditorAPI.Files.GetOpenedEditors().Select(e => e.Editor).ToList();

        textEditors.ForEach(textEditor => textEditor.WordWrap = SkEditorAPI.Core.GetAppConfig().IsWrappingEnabled);
    }
    
    private void ToggleZoomSync()
    {
        ToggleSetting("IsZoomSyncEnabled");

        if (!SkEditorAPI.Core.GetAppConfig().IsZoomSyncEnabled) return;

        List<TextEditor> textEditors = SkEditorAPI.Files.GetOpenedEditors().Select(e => e.Editor).ToList();
        double fontSize = textEditors.First().FontSize;
        textEditors.ForEach(textEditor =>
        {
            textEditor.FontSize = fontSize;
        });
    }

    private static void ToggleSetting(string propertyName)
    {
        var appConfig = SkEditorAPI.Core.GetAppConfig();
        var property = appConfig.GetType().GetProperty(propertyName);

        if (property == null || property.PropertyType != typeof(bool)) return;
        var currentValue = (bool?)property.GetValue(appConfig);
        property.SetValue(appConfig, !currentValue);
    }
}
