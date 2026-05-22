using System.Collections.Generic;
using System.Linq;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings.Types;

public class CompoundSetting(TranslatableString name) : SettingElement<CompoundSetting>(name)
{
    public List<SettingElement> Children { get; } = [];

    public bool IsVertical { get; set; }

    public override double Width => Children.Count == 0 ? base.Width : Children.Max(c => c.Width);

    public static CompoundSetting Create(TranslatableString name)
    {
        return new CompoundSetting(name);
    }

    public CompoundSetting WithChild(SettingElement child)
    {
        child.Parent = this;
        Children.Add(child);
        return this;
    }

    public CompoundSetting WithChildren(params SettingElement[] children)
    {
        foreach (SettingElement settingElement in children)
        {
            settingElement.Parent = this;
        }

        Children.AddRange(children);
        return this;
    }

    public CompoundSetting WithVerticalLayout()
    {
        IsVertical = true;
        return this;
    }
}