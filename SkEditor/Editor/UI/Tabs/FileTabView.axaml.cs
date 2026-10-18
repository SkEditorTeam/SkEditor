using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AvaloniaEdit.Editing;

namespace SkEditor.Editor.UI.Tabs;

public partial class FileTabView : UserControl
{
    public FileTabView()
    {
        InitializeComponent();

        AddHandler<FocusChangedEventArgs>(
            GotFocusEvent,
            (_, e) =>
            {
                if (e.Source is TextArea)
                {
                    (DataContext as FileTabViewModel)?.FocusCommand.Execute(null);
                }
            },
            RoutingStrategies.Bubble,
            true);
    }
}