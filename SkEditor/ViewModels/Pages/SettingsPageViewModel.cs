using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
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
            "General",
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
            ],
            [
                new SettingSection(
                    "Notifications",
                    new SymbolIconSource { Symbol = Symbol.Alert },
                    [
                        new ToggleSetting(
                            "Enable Notifications",
                            new SymbolIconSource { Symbol = Symbol.Alert },
                            "Receive notifications for important updates and alerts.",
                            true),
                        new ToggleSetting(
                            "Sound Alerts",
                            new SymbolIconSource { Symbol = Symbol.Headphones },
                            "Play a sound for notifications.",
                            false)
                    ]),
                new SettingSection(
                    "Backup",
                    new SymbolIconSource { Symbol = Symbol.Shield },
                    [
                        new ToggleSetting(
                            "Enable Backup",
                            new SymbolIconSource { Symbol = Symbol.Shield },
                            "Automatically back up files to prevent data loss.",
                            true),
                        new ActionSetting(
                            "Backup Settings",
                            new SymbolIconSource { Symbol = Symbol.Settings },
                            "Configure backup frequency and location.",
                            "Open Backup Settings")
                    ])
            ]
        ),

        new(
            "Appearance",
            new SymbolIconSource { Symbol = Symbol.Color },
            [
                new ToggleSetting(
                    "Enable Dark Mode",
                    new SymbolIconSource { Symbol = Symbol.Color },
                    "Switch the application theme to dark mode.",
                    false),
                new ActionSetting(
                    "Customize Theme",
                    new SymbolIconSource { Symbol = Symbol.PaintBrush },
                    "Open the theme customization settings.",
                    "Open Theme Settings")
            ])
    ];
    
    public SettingsPageViewModel()
    {
        _currentSection = SettingsCategories[0];
    }
}

public class SettingSection(
    string name,
    IconSource icon,
    List<SettingElement> settings,
    List<SettingSection>? subsections = null)
{
    public string Name { get; set; } = name;
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