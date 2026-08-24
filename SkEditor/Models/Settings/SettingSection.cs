using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings;

public class SettingSection(string id, TranslatableString name) : ObservableObject
{
    public StandardIcon Icon { get; private set; }
    public string Id { get; init; } = id;

    public BindableReactiveProperty<bool> IsExpanded { get; set; } = new(false);
    public BindableReactiveProperty<bool> IsVisible { get; set; } = new(true);
    public TranslatableString Name { get; init; } = name;

    public bool Selectable => !(Subsections.Count > 0 || Settings.Count == 0);
    public ObservableCollection<SettingElement> Settings { get; private set; } = [];
    public ObservableCollection<SettingSection> Subsections { get; private set; } = [];

    public static SettingSection Create(string id, TranslatableString name)
    {
        return new SettingSection(id, name);
    }

    public SettingSection WithIcon(StandardIcon icon)
    {
        Icon = icon;
        return this;
    }

    public SettingSection WithSettings(ObservableCollection<SettingElement> settings)
    {
        Settings = settings;
        return this;
    }

    public SettingSection WithSubsections(ObservableCollection<SettingSection> subsections)
    {
        Subsections = subsections;
        return this;
    }
}