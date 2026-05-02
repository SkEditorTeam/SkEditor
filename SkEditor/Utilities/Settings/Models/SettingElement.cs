using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models;

public abstract class SettingElement(TranslatableString name)
    : ObservableObject
{
    public TranslatableString Name { get; set; } = name;
    public SettingIcon? Icon { get; protected set; }
    public TranslatableString? Description { get; protected set; }

    public SettingElement? Parent { get; set; }

    public virtual double Width { get; set; } = double.NaN;
}

public abstract class SettingElement<T>(TranslatableString name) : SettingElement(name)
    where T : SettingElement<T>
{
    public T WithIcon(SettingIcon icon)
    {
        Icon = icon;
        return (T)this;
    }

    public T WithDescription(TranslatableString description)
    {
        Description = description;
        return (T)this;
    }
}