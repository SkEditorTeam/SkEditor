using System.Collections.Generic;
using System.Linq;
using R3;
using SkEditor.Core.Localization;

namespace SkEditor.Configuration.Models.Types;

public class CompoundSetting(BindableReactiveProperty<TranslatableString> name) : SettingElement<CompoundSetting>(name)
{
    public List<SettingElement> Children { get; } = [];

    public bool IsVertical { get; set; }

    public override double? Width => Children.Count == 0 ? base.Width : Children.Max(c => c.Width);

    public static CompoundSetting Create(TranslatableString name)
    {
        return new CompoundSetting(new BindableReactiveProperty<TranslatableString>(name));
    }

    public static CompoundSetting Create(BindableReactiveProperty<TranslatableString> name)
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