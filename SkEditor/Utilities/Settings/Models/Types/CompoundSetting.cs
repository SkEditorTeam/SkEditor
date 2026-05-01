using System.Collections.Generic;
using System.Linq;
using Avalonia.Layout;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public class CompoundSetting(TranslatableString name) : SettingElement(name)
{
    public List<SettingElement> Children { get; } = [];

    public bool IsVertical { get; set; }
    public Orientation LayoutOrientation => IsVertical ? Orientation.Vertical : Orientation.Horizontal;

    public HorizontalAlignment ChildrenHorizontalAlignment =>
        LayoutOrientation == Orientation.Vertical
            ? HorizontalAlignment.Stretch
            : HorizontalAlignment.Left;

    public override double Width => Children.Count == 0 ? base.Width : Children.Max(c => c.Width);

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

    public CompoundSetting WithVerticalLayout()
    {
        IsVertical = true;
        return this;
    }
}