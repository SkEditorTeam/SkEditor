using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models;

public abstract class SettingElement(TranslatableString name, SettingIcon icon, TranslatableString? description = null)
    : ObservableObject
{
    public TranslatableString Name { get; set; } = name;
    public SettingIcon Icon { get; set; } = icon;
    public TranslatableString? Description { get; set; } = description;
}