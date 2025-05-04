using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Syntax;

namespace SkEditor.Views.Settings.Personalization;

public partial class FileSyntaxes : UserControl
{
    public FileSyntaxes()
    {
        InitializeComponent();

        AssignCommands();
        LoadSyntaxes();
    }

    private void LoadSyntaxes()
    {
        List<FileSyntax> syntaxes = SyntaxLoader.FileSyntaxes;
        List<string> availableLangNames = syntaxes.Select(x => x.Config.LanguageName).Distinct().ToList();

        foreach (string langName in availableLangNames)
        {
            string? selectedSyntax = SkEditorAPI.Core.GetAppConfig().FileSyntaxes
                .FirstOrDefault(x => x.Key.Equals(langName)).Value ?? null;

            SettingsExpander expander = GenerateExpander(langName, selectedSyntax);
            SyntaxesStackPanel.Children.Add(expander);
        }
    }

    private SettingsExpander GenerateExpander(string language, string selectedSyntaxFullIdName)
    {
        ComboBox comboBox = new() { Name = language };
        SettingsExpander expander = new()
        {
            Header = language,
            IconSource = new SymbolIconSource { Symbol = Symbol.Code },
            Footer = comboBox
        };

        List<FileSyntax> fileSyntaxes =
            SyntaxLoader.FileSyntaxes.Where(x => x.Config.LanguageName.Equals(language)).ToList();

        foreach (FileSyntax syntax in fileSyntaxes)
        {
            ComboBoxItem newItem = new()
            {
                Content = syntax.Config.SyntaxName,
                Tag = syntax.Config.FullIdName
            };
            comboBox.Items.Add(newItem);
            if (syntax.Config.FullIdName.Equals(selectedSyntaxFullIdName))
            {
                comboBox.SelectedItem = newItem;
            }
        }

        if (comboBox.SelectedItem == null)
        {
            comboBox.SelectedIndex = 0;
        }

        comboBox.SelectionChanged += (_, _) =>
        {
            AppConfig config = SkEditorAPI.Core.GetAppConfig();
            string? selectedFullIdName = (comboBox.SelectedValue as ComboBoxItem).Tag.ToString();
            FileSyntax? selectedFileSyntax =
                SyntaxLoader.FileSyntaxes.FirstOrDefault(x => x.Config.FullIdName.Equals(selectedFullIdName));

            config.FileSyntaxes[selectedFileSyntax.Config.LanguageName] = selectedFileSyntax.Config.FullIdName;

            List<TabViewItem> tabs = SkEditorAPI.Files.GetOpenedFiles()
                .Where(o => o.IsEditor)
                .Where(o =>
                {
                    string ext = Path.GetExtension(o.Path?.ToLower() ?? "");
                    return selectedFileSyntax.Config.Extensions.Contains(ext);
                })
                .Select(o => o.TabViewItem)
                .ToList();

            foreach (TabViewItem tab in tabs)
            {
                TextEditor? editor = tab.Content as TextEditor;
                editor.SyntaxHighlighting = selectedFileSyntax.Highlighting;
            }
        };

        return expander;
    }

    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(PersonalizationPage)));
    }
}