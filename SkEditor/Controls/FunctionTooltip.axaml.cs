using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Media;
using SkEditor.Utilities.Parser;
using SkEditor.Utilities.Styling;

namespace SkEditor.Controls;

public partial class FunctionTooltip : UserControl, INotifyPropertyChanged
{
    public FunctionTooltip(SkDocParser.SkDocFunction skDocFunction)
    {
        InitializeComponent();
        DataContext = this;

        FunctionContentPanel.Children.Add(BuildFunctionContent(skDocFunction));

        SelectableTextBlock[] selectableTextBlocks = this.GetLogicalDescendants().OfType<SelectableTextBlock>().ToArray();
        foreach (var selectableTextBlock in selectableTextBlocks)
        {
            selectableTextBlock.SelectionBrush = ThemeEditor.CurrentTheme.SelectionColor;
            selectableTextBlock.ContextFlyout = null;
        }
    }

    private static TextBlock CreateTextBlock(string text, string color)
    {
        return new TextBlock
        {
            FontFamily = new FontFamily("JetBrains Mono"),
            FontSize = 13,
            Foreground = new SolidColorBrush(Color.Parse(color)),
            FontWeight = FontWeight.Bold,
            MaxWidth = 300,
            Text = text
        };
    }

    public static StackPanel BuildFunctionContent(SkDocParser.SkDocFunction skDocFunction)
    {
        var functionContentPanel = new StackPanel { Orientation = Orientation.Vertical };

        var headerStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
        headerStackPanel.Children.Add(CreateTextBlock("function", "#ff6542"));
        headerStackPanel.Children.Add(CreateTextBlock(" " + skDocFunction.Name, "#90dbf5"));
        headerStackPanel.Children.Add(CreateTextBlock("(", "#cdcaca"));
        functionContentPanel.Children.Add(headerStackPanel);

        int maxParamNameLength = skDocFunction.Parameters.Max(p => p.Name.Length);
        foreach (var parameter in skDocFunction.Parameters)
        {
            var parameterStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            string alignedParameterName = parameter.Name;
            alignedParameterName = alignedParameterName.PadRight(maxParamNameLength + 1);
            parameterStackPanel.Children.Add(CreateTextBlock("    " + alignedParameterName, "#d9ad69"));
            parameterStackPanel.Children.Add(CreateTextBlock(parameter.Type, "#54d7a9"));
            functionContentPanel.Children.Add(parameterStackPanel);
        }

        var footerStackPanel = new StackPanel { Orientation = Orientation.Horizontal };
        footerStackPanel.Children.Add(CreateTextBlock(")", "#cdcaca"));
        if (!string.IsNullOrEmpty(skDocFunction.ReturnType))
        {
            footerStackPanel.Children.Add(CreateTextBlock(" ::", "#cdcaca"));
            footerStackPanel.Children.Add(CreateTextBlock(" " + skDocFunction.ReturnType, "#54d7a9"));
        }
        functionContentPanel.Children.Add(footerStackPanel);

        return functionContentPanel;
    }
}