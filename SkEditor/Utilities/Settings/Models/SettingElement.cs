using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models;

public abstract class SettingElement(TranslatableString name, IconSource icon, TranslatableString? description = null)
    : ObservableObject
{
    public TranslatableString Name { get; set; } = name;
    public IconSource Icon { get; set; } = icon;
    public TranslatableString? Description { get; set; } = description;
}