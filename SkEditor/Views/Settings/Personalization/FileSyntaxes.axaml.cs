using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using Microsoft.CodeAnalysis;
using SkEditor.API;
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
        var syntaxes = SyntaxLoader.FileSyntaxes;
        var availableLangNames = syntaxes.Select(x => x.Config.LanguageName).Distinct().ToList();
        
        foreach (string langName in availableLangNames)
        {
            var selectedSyntax = ApiVault.Get().GetAppConfig().FileSyntaxes.FirstOrDefault(x => x.Key.Equals(langName)).Value ?? null;
            
            var expander = GenerateExpander(langName,
                syntaxes.Where(x => x.Config.LanguageName.Equals(langName)).Select(x => x.Config.SyntaxName).ToArray(), selectedSyntax);
            SyntaxesStackPanel.Children.Add(expander);
        }
    }

    private SettingsExpander GenerateExpander(string name, string[] values, string? selectedValue)
    {
        var comboBox = new ComboBox { Name = name };
        var expander = new SettingsExpander
        {
            Header = name,
            IconSource = new SymbolIconSource { Symbol = Symbol.Code },
            Footer = comboBox
        };

        foreach (string value in values)
        {
            comboBox.Items.Add(value);
        }
        
        comboBox.SelectedItem = selectedValue ?? values[0];
        comboBox.SelectionChanged += (_, _) =>
        {
            var config = ApiVault.Get().GetAppConfig();
            config.FileSyntaxes[name] = comboBox.SelectedItem.ToString();
        };

        return expander;
    }

    private void AssignCommands()
    {
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(PersonalizationPage)));
    }
}