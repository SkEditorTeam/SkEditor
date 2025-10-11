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
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Views.Windows.Settings.Personalization;

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
        List<string> availableLangNames = syntaxes
            .Where(x => x.Config != null)
            .Select(x => x.Config!.LanguageName)
            .Distinct()
            .ToList();

        foreach (string langName in availableLangNames)
        {
            string? selectedSyntax = SkEditorAPI.Core.GetAppConfig().FileSyntaxes
                .FirstOrDefault(x => x.Key.Equals(langName)).Value ?? null;

            if (string.IsNullOrEmpty(selectedSyntax))
            {
                continue;
            }

            SettingsExpander expander = GenerateExpander(langName, selectedSyntax);
            SyntaxesStackPanel.Children.Add(expander);
        }
    }

    private static SettingsExpander GenerateExpander(string language, string selectedSyntaxFullIdName)
    {
        ComboBox comboBox = new() { Name = language };
        SettingsExpander expander = new()
        {
            Header = language,
            IconSource = new SymbolIconSource() { Symbol = Symbol.Code },
            Footer = comboBox
        };

        List<FileSyntax> fileSyntaxes =
            SyntaxLoader.FileSyntaxes.Where(x => x.Config != null && x.Config.LanguageName.Equals(language)).ToList();

        foreach (FileSyntax syntax in fileSyntaxes)
        {
            ComboBoxItem newItem = new()
            {
                Content = syntax.Config?.SyntaxName,
                Tag = syntax.Config?.FullIdName
            };
            comboBox.Items.Add(newItem);
            if (syntax.Config?.FullIdName.Equals(selectedSyntaxFullIdName) == true)
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
            string? selectedFullIdName = (comboBox.SelectedValue as ComboBoxItem)?.Tag?.ToString();
            FileSyntax? selectedFileSyntax =
                SyntaxLoader.FileSyntaxes.FirstOrDefault(x => x.Config != null && x.Config.FullIdName.Equals(selectedFullIdName));

            var selectedFileSyntaxConfig = selectedFileSyntax?.Config;
            if (selectedFileSyntaxConfig == null) return;
            config.FileSyntaxes[language] = selectedFileSyntaxConfig.FullIdName;
            config.FileSyntaxes[selectedFileSyntaxConfig.LanguageName] = selectedFileSyntaxConfig.FullIdName;

            List<TabViewItem?> tabs = SkEditorAPI.Files.GetOpenedEditors()
                .Where(o =>
                {
                    string ext = Path.GetExtension(o.Path?.ToLower() ?? "");
                    return selectedFileSyntaxConfig.Extensions.Contains(ext);
                })
                .Select(o => o.TabViewItem)
                .ToList();

            foreach (TabViewItem? tab in tabs)
            {
                if (tab?.Content is not TextEditor editor) continue;
                editor.SyntaxHighlighting = selectedFileSyntax?.Highlighting;
            }
        };

        return expander;
    }

    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(PersonalizationPage)));
    }
}