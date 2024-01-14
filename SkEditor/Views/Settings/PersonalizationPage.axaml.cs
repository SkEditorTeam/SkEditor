using Avalonia;
using Avalonia.Controls;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Syntax;
using SkEditor.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SkEditor.Views.Settings.Personalization;

namespace SkEditor.Views.Settings;
public partial class PersonalizationPage : UserControl
{
    public PersonalizationPage()
    {
        InitializeComponent();

        DataContext = new SettingsViewModel();

        AssignCommands();
    }

    private void AssignCommands()
    {
        ThemePageButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(ThemePage)));
        SyntaxPageButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(FileSyntaxes)));
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));

        FontButton.Command = new RelayCommand(SelectFont);
    }

    private async void SelectFont()
    {
        FontSelectionWindow window = new();
        string result = await window.ShowDialog<string>(ApiVault.Get().GetMainWindow());
        if (result is null)
            return;

        ApiVault.Get().GetAppConfig().Font = result;
        CurrentFont.Description = Translation.Get("SettingsPersonalizationFontDescription").Replace("{0}", result);

        List<TextEditor> textEditors = ApiVault.Get().GetTabView().TabItems
            .Cast<TabViewItem>()
            .Where(i => i.Content is TextEditor)
            .Select(i => i.Content as TextEditor).ToList();

        textEditors.ForEach(i =>
        {
            if (result.Equals("Default"))
            {
                Application.Current.TryGetResource("JetBrainsFont", Avalonia.Styling.ThemeVariant.Default, out object font);
                i.FontFamily = (Avalonia.Media.FontFamily)font;
            }
            else
            {
                i.FontFamily = new(result);
            }
        });
    }
}
