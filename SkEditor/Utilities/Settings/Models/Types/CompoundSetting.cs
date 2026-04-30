using System.Collections.Generic;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public class CompoundSetting(TranslatableString name) : SettingElement(name)
{
    public List<SettingElement> Children { get; } = [];

    public static CompoundSetting Create(TranslatableString name) => new(name);

    public CompoundSetting WithIcon(SettingIcon icon)
    {
        Icon = icon;
        return this;
    }

    public CompoundSetting WithDescription(TranslatableString description)
    {
        Description = description;
        return this;
    }

    public CompoundSetting WithChild(SettingElement child)
    {
        Children.Add(child);
        return this;
    }

    public CompoundSetting WithChildren(params SettingElement[] children)
    {
        Children.AddRange(children);
        return this;
    }
}