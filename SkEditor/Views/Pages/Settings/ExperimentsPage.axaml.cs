using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Settings;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Views.Pages.Settings;

public partial class ExperimentsPage : UserControl
{
    private readonly List<Experiment> _experiments;

    private readonly Dictionary<string, ToggleSwitch> _switches = new();

    public ExperimentsPage()
    {
        InitializeComponent();

        ExperimentsConfig experimentsConfig = SkEditorAPI.Configs.GetExperimentsConfig();

        _experiments =
        [
            new Experiment("Auto Completion", "Early prototype of auto completion, not very helpful.",
                () => experimentsConfig.AutoCompletionPrototypeEnabled,
                flag => experimentsConfig.AutoCompletionPrototypeEnabled = flag,
                Symbol.TextGrammarWand),
            new Experiment("Projects", "Adds a sidebar for managing projects.",
                () => experimentsConfig.ProjectsEnabled,
                flag => experimentsConfig.ProjectsEnabled = flag, Symbol.Briefcase),
            new Experiment("Hex Preview", "Preview hex colors in the editor.",
                () => experimentsConfig.HexPreviewEnabled,
                flag => experimentsConfig.HexPreviewEnabled = flag, Symbol.Color),
            new Experiment("Code Parser",
                "Parse code for informations. Doesn't contain error checking, see Analyzer addon instead. Requires Projects experiment.",
                () => experimentsConfig.CodeParserEnabled, flag => experimentsConfig.CodeParserEnabled = flag,
                Symbol.DocumentSearch, "Projects"),
            new Experiment("Real-Time Code Parser",
                "Automatically parses your code with every change you make. Requires Code Parser experiment.",
                () => experimentsConfig.RealtimeCodeParserEnabled,
                flag => experimentsConfig.RealtimeCodeParserEnabled = flag, Symbol.ArrowSync, "Code Parser"),
            new Experiment("Folding", "Folding code blocks. Requires Code Parser experiment.",
                () => experimentsConfig.FoldingEnabled, flag => experimentsConfig.FoldingEnabled = flag,
                Symbol.TextCollapse,
                "Code Parser"),
            new Experiment("Session restoring",
                "Automatically saves your files and reopens it next time you start the app."
                , () => experimentsConfig.SessionRestoringEnabled,
                flag => experimentsConfig.SessionRestoringEnabled = flag, Symbol.History)
        ];

        AddExperiments();
    }

    private void AddExperiments()
    {
        foreach (Experiment experiment in _experiments)
        {
            ToggleSwitch toggleSwitch = new()
            {
                IsChecked = experiment.GetFlag()
            };

            toggleSwitch.IsCheckedChanged += (_, _) => Switch(experiment, toggleSwitch);

            SettingsExpander expander = new()
            {
                Header = experiment.Name,
                Description = experiment.Description,
                Footer = toggleSwitch,
                IconSource = new SymbolIconSource { Symbol = experiment.Icon }
            };

            _switches[experiment.Name] = toggleSwitch;
            ExperimentsStackPanel.Children.Add(expander);
        }
    }

    private void Switch(Experiment experiment, ToggleSwitch toggleSwitch)
    {
        if (toggleSwitch.IsChecked == true)
        {
            if (experiment.Dependency is not null &&
                _experiments.FirstOrDefault(e => e.Name == experiment.Dependency) is { } dependency &&
                _switches.TryGetValue(dependency.Name, out ToggleSwitch? dependencySwitch) &&
                dependencySwitch.IsChecked != true)
            {
                dependencySwitch.IsChecked = true;
                dependency.SetFlag(true);
            }
        }
        else
        {
            IEnumerable<Experiment> dependentExperiments = _experiments.Where(e => e.Dependency == experiment.Name);

            foreach (Experiment dependent in dependentExperiments)
            {
                if (!_switches.TryGetValue(dependent.Name, out ToggleSwitch? dependentSwitch) ||
                    dependentSwitch.IsChecked != true)
                {
                    continue;
                }

                dependentSwitch.IsChecked = false;
                dependent.SetFlag(false);
            }
        }

        experiment.SetFlag(toggleSwitch.IsChecked == true);
        _ = SkEditorAPI.Configs.GetExperimentsConfig().Save();
    }
}

public record Experiment(
    string Name,
    string Description,
    Func<bool> GetFlag,
    Action<bool> SetFlag,
    Symbol Icon,
    string? Dependency = null);