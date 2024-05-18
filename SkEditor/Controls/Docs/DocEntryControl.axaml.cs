using Avalonia;
using Avalonia.Controls;

namespace SkEditor.Controls.Docs;

public partial class DocEntryControl : UserControl
{
    
    public static readonly StyledProperty<string> EntryTitleProperty =
        AvaloniaProperty.Register<DocEntryControl, string>(nameof(EntryTitle));
    public static readonly StyledProperty<Control> EntryContentProperty =
        AvaloniaProperty.Register<DocEntryControl, Control>(nameof(EntryContent));
    public static readonly StyledProperty<bool> IsExpandedProperty =
        AvaloniaProperty.Register<DocEntryControl, bool>(nameof(IsExpanded));

    public string EntryTitle
    {
        get => GetValue(EntryTitleProperty);
        set => SetValue(EntryTitleProperty, value);
    }
    
    public Control EntryContent 
    {
        get => GetValue(EntryContentProperty);
        set => SetValue(EntryContentProperty, value);
    }
    
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
    
    public DocEntryControl()
    {
        InitializeComponent();
        
        DataContext = this;
    }
}