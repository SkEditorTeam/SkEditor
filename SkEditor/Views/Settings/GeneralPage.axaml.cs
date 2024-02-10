using Avalonia.Controls;
using Avalonia.Threading;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SkEditor.Views.Settings;
public partial class GeneralPage : UserControl
{
    public GeneralPage()
    {
        InitializeComponent();

        DataContext = new SettingsViewModel();

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
        LanguageComboBox.SelectedItem = ApiVault.Get().GetAppConfig().Language;
        LanguageComboBox.SelectionChanged += (s, e) =>
        {
            string language = LanguageComboBox.SelectedItem.ToString();
            ApiVault.Get().GetAppConfig().Language = language;
            Dispatcher.UIThread.InvokeAsync(() => Translation.ChangeLanguage(language));
        };
    }

    private void LoadIndentation()
    {
        var appConfig = ApiVault.Get().GetAppConfig();
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
        AutoIndentToggleSwitch.Command = new RelayCommand(() => ToggleSetting("IsAutoIndentEnabled"));
        AutoPairingToggleSwitch.Command = new RelayCommand(() => ToggleSetting("IsAutoPairingEnabled"));
        AutoSaveToggleSwitch.Command = new RelayCommand(() => ToggleSetting("IsAutoSaveEnabled"));
        CheckForUpdatesToggleSwitch.Command = new RelayCommand(() => ToggleSetting("CheckForUpdates"));
        CheckForChangesToggleSwitch.Command = new RelayCommand(() => ToggleSetting("CheckForChanges"));
        IndentationAmountComboBox.SelectionChanged += (s, e) =>
        {
            var appConfig = ApiVault.Get().GetAppConfig();
            appConfig.TabSize = int.Parse((IndentationAmountComboBox.SelectedItem as ComboBoxItem).Tag.ToString());
            ApiVault.Get().GetOpenedEditors().ForEach(e => e.Options.IndentationSize = appConfig.TabSize);
        };
        IndentationTypeComboBox.SelectionChanged += (s, e) =>
        {
            var appConfig = ApiVault.Get().GetAppConfig();
            appConfig.UseSpacesInsteadOfTabs = (IndentationTypeComboBox.SelectedItem as ComboBoxItem).Tag.ToString() == "spaces";
            ApiVault.Get().GetOpenedEditors().ForEach(e => e.Options.ConvertTabsToSpaces = appConfig.UseSpacesInsteadOfTabs);
        };
    }

    private void ToggleRpc()
    {
        ToggleSetting("IsDiscordRpcEnabled");

        if (ApiVault.Get().GetAppConfig().IsDiscordRpcEnabled) DiscordRpcUpdater.Initialize();
        else DiscordRpcUpdater.Uninitialize();
    }

    private void ToggleWrapping()
    {
        ToggleSetting("IsWrappingEnabled");

        List<TextEditor> textEditors = ApiVault.Get().GetTabView().TabItems
            .OfType<TabViewItem>()
            .Select(x => x.Content as TextEditor)
            .Where(editor => editor != null)
            .ToList();

        textEditors.ForEach(e => e.WordWrap = ApiVault.Get().GetAppConfig().IsWrappingEnabled);
    }

    private static void ToggleSetting(string propertyName)
    {
        var appConfig = ApiVault.Get().GetAppConfig();
        var property = appConfig.GetType().GetProperty(propertyName);

        if (property == null || property.PropertyType != typeof(bool)) return;
        var currentValue = (bool)property.GetValue(appConfig);
        property.SetValue(appConfig, !currentValue);
    }
}
