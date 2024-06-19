using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
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
        LanguageComboBox.SelectionChanged += (s, e) =>
        {
            string language = LanguageComboBox.SelectedItem.ToString();
            SkEditorAPI.Core.GetAppConfig().Language = language;
            Dispatcher.UIThread.InvokeAsync(() => Translation.ChangeLanguage(language));
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
        ProjectSingleClickToggleSwitch.Command = new RelayCommand(() => ToggleSetting("IsProjectSingleClickEnabled"));

        IndentationAmountComboBox.SelectionChanged += (s, e) =>
        {
            var appConfig = SkEditorAPI.Core.GetAppConfig();
            appConfig.TabSize = int.Parse((IndentationAmountComboBox.SelectedItem as ComboBoxItem).Tag.ToString());
            SkEditorAPI.Files.GetOpenedEditors().ForEach(e => e.Editor.Options.IndentationSize = appConfig.TabSize);
        };
        IndentationTypeComboBox.SelectionChanged += (s, e) =>
        {
            var appConfig = SkEditorAPI.Core.GetAppConfig();
            appConfig.UseSpacesInsteadOfTabs = (IndentationTypeComboBox.SelectedItem as ComboBoxItem).Tag.ToString() == "spaces";
            SkEditorAPI.Files.GetOpenedEditors().ForEach(e => e.Editor.Options.ConvertTabsToSpaces = appConfig.UseSpacesInsteadOfTabs);
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

        textEditors.ForEach(e => e.WordWrap = SkEditorAPI.Core.GetAppConfig().IsWrappingEnabled);
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
