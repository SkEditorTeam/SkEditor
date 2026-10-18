using Avalonia.Controls;
using SkEditor.Configuration.Models.Types;

namespace SkEditor.Configuration.UI;

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
            InfoSetting => "InfoExpanderItemTemplate",
            _ => "StandardExpanderItemTemplate"
        };
    }
}