using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using SkEditor.Models.UI;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings;

public class SettingSection(TranslatableString name) : ObservableObject
{
    public TranslatableString Name { get; init; } = name;
    public StandardIcon Icon { get; private set; }
    public List<SettingElement> Settings { get; private set; } = [];
    public List<SettingSection> Subsections { get; private set; } = [];

    public BindableReactiveProperty<bool> IsVisible { get; set; } = new(true);

    public bool Selectable => !(Subsections.Count > 0 || Settings.Count == 0);

    public static SettingSection Create(TranslatableString name)
    {
        return new SettingSection(name);
    }

    public SettingSection WithIcon(StandardIcon icon)
    {
        Icon = icon;
        return this;
    }

    public SettingSection WithSettings(List<SettingElement> settings)
    {
        Settings = settings;
        return this;
    }

    public SettingSection WithSubsections(List<SettingSection> subsections)
    {
        Subsections = subsections;
        return this;
    }
}