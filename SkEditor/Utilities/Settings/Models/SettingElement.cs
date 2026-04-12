using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models;

public abstract class SettingElement(TranslatableString name, FAIconSource icon, TranslatableString? description = null)
    : ObservableObject
{
    public TranslatableString Name { get; set; } = name;
    public FAIconSource Icon { get; set; } = icon;
    public TranslatableString? Description { get; set; } = description;
}