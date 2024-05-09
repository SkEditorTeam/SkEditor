using System.ComponentModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Styling;

namespace SkEditor.Controls;

public partial class FunctionTooltip : UserControl, INotifyPropertyChanged
{
    public FunctionTooltip()
    {
        InitializeComponent();
        DataContext = this;

        SelectableTextBlock[] selectableTextBlocks = this.GetLogicalDescendants().OfType<SelectableTextBlock>().ToArray();
        foreach (var selectableTextBlock in selectableTextBlocks)
        {
            selectableTextBlock.SelectionBrush = ThemeEditor.CurrentTheme.SelectionColor;
            selectableTextBlock.ContextFlyout = null;
        }
    }
}