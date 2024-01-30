using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;

namespace SkEditor.Views.FileTypes;

public partial class AssociationItemView : UserControl
{
    
    public static readonly AvaloniaProperty<string> AssociationSource = AvaloniaProperty.Register<AssociationItemView, string>(nameof(AssociationSource));
    public static readonly AvaloniaProperty<string> AssociationDescription = AvaloniaProperty.Register<AssociationItemView, string>(nameof(AssociationDescription));
    
    public string Source
    {
        get => GetValue(AssociationSource)?.ToString();
        set => SetValue(AssociationSource, value);
    }
    
    public string Description
    {
        get => GetValue(AssociationDescription)?.ToString();
        set => SetValue(AssociationDescription, value);
    }

    public AssociationItemView()
    {
        InitializeComponent();
        
        DataContext = this;
    }

    public void UpdateIcon(Symbol symbol)
    {
        Icon.Symbol = symbol;
    }
}