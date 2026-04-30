using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.Templates;
using SkEditor.Utilities.Settings.Models;
using SkEditor.Utilities.Settings.Models.Types;

namespace SkEditor.Views.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }
}

public class SettingElementTemplateSelector : IDataTemplate
{
    public DataTemplate StandardTemplate { get; set; } = null!;
    public DataTemplate ClickableTemplate { get; set; } = null!;

    public bool Match(object? data)
    {
        return data is SettingElement;
    }

    public Control? Build(object? param)
    {
        return param is ClickableActionSetting
            ? ClickableTemplate.Build(param)
            : StandardTemplate.Build(param);
    }
}