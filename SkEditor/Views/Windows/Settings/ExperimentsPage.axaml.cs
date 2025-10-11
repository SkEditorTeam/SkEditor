using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Views.Windows.Settings;

public partial class ExperimentsPage : UserControl
{
    private readonly List<Experiment> _experiments =
    [
        new("Auto Completion", "Early prototype of auto completion, not very helpful.",
            "EnableAutoCompletionExperiment",  Symbol.TextGrammarWand),
        new("Projects", "Adds a sidebar for managing projects.", "EnableProjectsExperiment", Symbol.Briefcase),
        new("Hex Preview", "Preview hex colors in the editor.", "EnableHexPreview", Symbol.Color),
        new("Code Parser",
            "Parse code for informations. Doesn't contain error checking, see Analyzer addon instead. Requires Projects experiment.",
            "EnableCodeParser", Symbol.DocumentSearch, "EnableProjectsExperiment"),
        new("Real-Time Code Parser",
            "Automatically parses your code with every change you make. Requires Code Parser experiment.",
            "EnableRealtimeCodeParser", Symbol.ArrowSync, "EnableCodeParser"),
        new("Folding", "Folding code blocks. Requires Code Parser experiment.", "EnableFolding", Symbol.TextCollapse,
            "EnableCodeParser"),
        //new Experiment("Better pairing", "Experimental better version of auto pairing.", "EnableBetterPairing", "AutoPairingIcon"),
        new("Session restoring", "Automatically saves your files and reopens it next time you start the app.",
            "EnableSessionRestoring", Symbol.History)
    ];
    private readonly Dictionary<string, Experiment> _experimentsByOption;
    private readonly Dictionary<string, ToggleSwitch> _switches = new();

    public ExperimentsPage()
    {
        InitializeComponent();
        
        _experimentsByOption = _experiments.ToDictionary(e => e.Option);

        AddExperiments();
    }

    private void AddExperiments()
    {
        AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();

        foreach (Experiment experiment in _experiments)
        {
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
                IconSource = new SymbolIconSource { Symbol = experiment.Icon}
            };
            
            _switches[experiment.Option] = toggleSwitch;
            ExperimentsStackPanel.Children.Add(expander);
        }
    }

    private void Switch(Experiment experiment, ToggleSwitch toggleSwitch)
    {
        AppConfig appConfig = SkEditorAPI.Core.GetAppConfig();
        
        if (toggleSwitch.IsChecked == true)
        {
            if (experiment.Dependency is not null &&
                _experimentsByOption.TryGetValue(experiment.Dependency, out var dependency) &&
                _switches.TryGetValue(dependency.Option, out var dependencySwitch) &&
                dependencySwitch.IsChecked != true)
            {
                dependencySwitch.IsChecked = true;
                appConfig.SetExperimentFlag(dependency.Option, true);
            }
        }
        else
        {
            var dependentExperiments = _experiments.Where(e => e.Dependency == experiment.Option);
            
            foreach (var dependent in dependentExperiments)
            {
                if (_switches.TryGetValue(dependent.Option, out var dependentSwitch) && dependentSwitch.IsChecked == true)
                {
                    dependentSwitch.IsChecked = false;
                    appConfig.SetExperimentFlag(dependent.Option, false);
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
    Symbol Icon,
    string? Dependency = null);