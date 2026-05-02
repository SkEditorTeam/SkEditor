using Avalonia.Controls;
using SkEditor.Utilities.Settings.Models.Types;

namespace SkEditor.DataTemplates;

public class SettingElementTemplateFactory : RecyclingElementFactory
{
    protected override string OnSelectTemplateKeyCore(object? dataContext, Control? owner)
    {
        return dataContext switch
        {
            ClickableActionSetting => "ClickableTemplate",
            ExpandableSetting => "ExpandableTemplate",
            _ => "StandardTemplate"
        };
    }
}