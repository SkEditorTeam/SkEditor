using Avalonia.Controls;
using SkEditor.Models.Settings.Types;

namespace SkEditor.DataTemplates;

public class SettingElementTemplateFactory : RecyclingElementFactory
{
    protected override string OnSelectTemplateKeyCore(object? dataContext, Control? owner)
    {
        return dataContext switch
        {
            ClickableActionSetting => "ClickableTemplate",
            ExpandableSetting => "ExpandableTemplate",
            InfoSetting => "InfoTemplate",
            _ => "StandardTemplate"
        };
    }
}

public class ExpanderItemTemplateFactory : RecyclingElementFactory
{
    protected override string OnSelectTemplateKeyCore(object? dataContext, Control? owner)
    {
        return dataContext switch
        {
            ClickableActionSetting => "ClickableExpanderItemTemplate",
            _ => "StandardExpanderItemTemplate"
        };
    }
}