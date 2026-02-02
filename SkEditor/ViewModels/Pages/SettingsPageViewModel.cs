using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.ViewModels.Pages;

public partial class SettingsPageViewModel : ObservableObject
{
    [ObservableProperty]
    private SettingSection _currentSection;

    public List<SettingSection> SettingsCategories { get; } =
    [
        new(
            Translation.GetObservable("SettingsGeneralTitle"),
            new SymbolIconSource { Symbol = Symbol.Settings },
            [
                new ToggleSetting(
                    "Enable Auto-Save",
                    new SymbolIconSource { Symbol = Symbol.Save },
                    "Automatically save files at regular intervals.",
                    true),
                new ActionSetting(
                    "Manage Plugins",
                    new SymbolIconSource { Symbol = Symbol.PuzzlePiece },
                    "Add, remove, or update plugins for additional functionality.",
                    "Open Plugin Manager")
            ])
    ];

    public SettingsPageViewModel()
    {
        _currentSection = SettingsCategories[0];
    }
}

public class SettingSection(
    TranslatableString name,
    IconSource icon,
    List<SettingElement> settings,
    List<SettingSection>? subsections = null)
{
    public TranslatableString Name { get; set; } = name;
    public IconSource Icon { get; set; } = icon;
    public List<SettingElement> Settings { get; set; } = settings;
    public List<SettingSection> Subsections { get; set; } = subsections ?? [];
}

public abstract class SettingElement(string name, IconSource icon, string description)
{
    public string Name { get; set; } = name;
    public IconSource Icon { get; set; } = icon;
    public string Description { get; set; } = description;
}

public class ToggleSetting(string name, IconSource icon, string description, bool isEnabled)
    : SettingElement(name, icon, description)
{
    public bool IsEnabled { get; set; } = isEnabled;
}

public class ActionSetting(string name, IconSource icon, string description, string buttonText)
    : SettingElement(name, icon, description)
{
    public string ButtonText { get; set; } = buttonText;
}