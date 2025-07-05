using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;

namespace SkEditor.Views.Settings;

public partial class ExperimentsPage : UserControl
{
    private readonly List<Experiment> _experiments =
    [
        new("Auto Completion", "Early prototype of auto completion, not very helpful.",
            "EnableAutoCompletionExperiment", "MagicWandIcon"),
        new("Projects", "Adds a sidebar for managing projects.", "EnableProjectsExperiment", "Folder"),
        new("Hex Preview", "Preview hex colors in the editor.", "EnableHexPreview", "ColorIcon"),
        new("Code Parser",
            "Parse code for informations. Doesn't contain error checking, see Analyzer addon instead. Requires Projects experiment.",
            "EnableCodeParser", "SearchIcon", "EnableProjectsExperiment"),
        new("Real-Time Code Parser",
            "Automatically parses your code with every change you make. Requires Code Parser experiment.",
            "EnableRealtimeCodeParser", "SearchIcon", "EnableCodeParser"),
        new("Folding", "Folding code blocks. Requires Code Parser experiment.", "EnableFolding", "FoldingIcon",
            "EnableCodeParser"),
        //new Experiment("Better pairing", "Experimental better version of auto pairing.", "EnableBetterPairing", "AutoPairingIcon"),
        new("Session restoring", "Automatically saves your files and reopens it next time you start the app.",
            "EnableSessionRestoring", "SessionRestoringIcon")
    ];

    public ExperimentsPage()
    {
        InitializeComponent();

        AddExperiments();
    }

    private void AddExperiments()
    {
        AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();

        foreach (Experiment experiment in _experiments)
        {
            object? icon = null;
            Application.Current?.TryGetResource(experiment.Icon, ThemeVariant.Default, out icon);

            ToggleSwitch toggleSwitch = new()
            {
                IsChecked = appConfig.GetExperimentFlag(experiment.Option)
            };

            toggleSwitch.IsCheckedChanged += (_, _) => Switch(experiment, toggleSwitch);

            SettingsExpander expander = new()
            {
                Header = experiment.Name,
                Description = experiment.Description,
                Footer = toggleSwitch,
                IconSource = icon as IconSource
            };

            ExperimentsStackPanel.Children.Add(expander);
        }
    }

    private void Switch(Experiment experiment, ToggleSwitch toggleSwitch)
    {
        AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();
        if (toggleSwitch.IsChecked == true && experiment.Dependency is not null)
        {
            Experiment? dependency = _experiments.Find(e => e.Option == experiment.Dependency);
            if (dependency is not null)
            {
                if (ExperimentsStackPanel.Children.OfType<SettingsExpander>()
                        .FirstOrDefault(e => e.Header?.ToString() == dependency.Name)
                        ?.Footer is ToggleSwitch dependencySwitch)
                {
                    if (dependencySwitch.IsChecked != true)
                    {
                        dependencySwitch.IsChecked = true;
                    }
                }
            }
        }

        appConfig.SetExperimentFlag(experiment.Option, toggleSwitch.IsChecked == true);
        appConfig.Save();
    }
}

public record Experiment(
    string Name,
    string Description,
    string Option,
    string Icon = "Settings",
    string? Dependency = null);