using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models;

public abstract class SettingElement(TranslatableString name)
    : ObservableObject
{
    public TranslatableString Name { get; set; } = name;
    public SettingIcon? Icon { get; protected set; }
    public TranslatableString? Description { get; protected set; }

    public virtual double Width { get; set; } = double.NaN;
}