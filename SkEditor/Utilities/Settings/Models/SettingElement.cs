using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models;

public abstract class SettingElement(TranslatableString name, Symbol icon, TranslatableString? description = null)
    : ObservableObject
{
    public TranslatableString Name { get; set; } = name;
    public Symbol Icon { get; set; } = icon;
    public TranslatableString? Description { get; set; } = description;
}