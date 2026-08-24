using System.Collections.ObjectModel;
using AvaloniaEdit.Utils;
using R3;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Settings.Types;

public class ExpandableSetting(BindableReactiveProperty<TranslatableString> name)
    : SettingElement<ExpandableSetting>(name)
{
    public ObservableCollection<SettingElement> Children { get; } = [];

    public static ExpandableSetting Create(TranslatableString name)
    {
        return new ExpandableSetting(new BindableReactiveProperty<TranslatableString>(name));
    }

    public static ExpandableSetting Create(BindableReactiveProperty<TranslatableString> name)
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