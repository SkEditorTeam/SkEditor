using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models;

public class SettingSection(
    TranslatableString name,
    Symbol icon,
    IEnumerable<SettingElement> settings,
    List<SettingSection>? subsections = null) : ObservableObject
{
    public TranslatableString Name { get; set; } = name;
    public Symbol Icon { get; set; } = icon;
    public List<SettingElement> Settings { get; set; } = settings.ToList();
    public List<SettingSection> Subsections { get; set; } = subsections ?? [];
}