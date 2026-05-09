using System.Collections.Generic;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings.Types;

public class ExpandableSetting(TranslatableString name) : SettingElement<ExpandableSetting>(name)
{
    public List<SettingElement> Children { get; } = [];

    public static ExpandableSetting Create(TranslatableString name)
    {
        return new ExpandableSetting(name);
    }

    public ExpandableSetting WithChild(SettingElement child)
    {
        child.Parent = this;
        Children.Add(child);
        return this;
    }

    public ExpandableSetting WithChildren(params SettingElement[] children)
    {
        foreach (SettingElement settingElement in children)
        {
            settingElement.Parent = this;
        }

        Children.AddRange(children);
        return this;
    }
}