using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Models.Types;

namespace SkEditor.DataTemplates;

public class SettingElementTemplateSelector : IDataTemplate
{
    public DataTemplate StandardTemplate { get; set; } = null!;
    public DataTemplate ClickableTemplate { get; set; } = null!;
    public DataTemplate ExpandableTemplate { get; set; } = null!;

    public bool Match(object? data)
    {
        return data is SettingElement;
    }

    public Control? Build(object? param)
    {
        return param switch
        {
            ClickableActionSetting => ClickableTemplate.Build(param),
            ExpandableSetting => ExpandableTemplate.Build(param),
            _ => StandardTemplate.Build(param)
        };
    }
}