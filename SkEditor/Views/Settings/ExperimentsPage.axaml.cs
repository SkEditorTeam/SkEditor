using Avalonia;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using System.Collections.Generic;

namespace SkEditor.Views.Settings;
public partial class ExperimentsPage : UserControl
{
    private readonly List<Experiment> experiments =
    [
        new Experiment("Auto Completion", "Early prototype of auto completion, not very helpful.", "EnableAutoCompletionExperiment", "MagicWandIcon"),
        new Experiment("Projects", "Adds a sidebar for managing projects.", "EnableProjectsExperiment", "Folder"),
        new Experiment("Hex Preview", "Preview hex colors in the editor.", "EnableHexPreview", "ColorIcon"),
        new Experiment("Code Parser", "Parse code for informations. Doesn't contain error checking, see Analyzer addon instead. Requires Projects experiment.", "EnableCodeParser", "SearchIcon"),
        new Experiment("Folding", "Folding code blocks. Requires Code Parser experiment.", "EnableFolding", "FoldingIcon"),
    ];

    public ExperimentsPage()
    {
        InitializeComponent();
        Title.BackButton.Command = new RelayCommand(() => SettingsWindow.NavigateToPage(typeof(HomePage)));

        AddExperiments();
    }

    private void AddExperiments()
    {
        foreach (var experiment in experiments)
        {
            Application.Current.TryGetResource(experiment.Icon, Avalonia.Styling.ThemeVariant.Default, out object icon);

            ToggleSwitch toggleSwitch = new()
            {
                IsChecked = ApiVault.Get().GetAppConfig().GetOptionValue<bool>(experiment.Option),
            };

            toggleSwitch.IsCheckedChanged += (sender, e) =>
            {
                ApiVault.Get().GetAppConfig().SetOptionValue(experiment.Option, toggleSwitch.IsChecked.Value);
                ApiVault.Get().GetAppConfig().Save();
            };

            SettingsExpander expander = new()
            {
                Header = experiment.Name,
                Description = experiment.Description,
                Footer = toggleSwitch,
                IconSource = icon as IconSource,
            };

            ExperimentsStackPanel.Children.Add(expander);
        }
    }
}

public record Experiment(string Name, string Description, string Option, string Icon = "Settings");