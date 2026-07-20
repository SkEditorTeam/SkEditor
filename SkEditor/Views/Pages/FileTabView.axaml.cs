using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaEdit.Editing;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Views.Pages;

public partial class FileTabView : UserControl
{
    public FileTabView()
    {
        InitializeComponent();

        Editor.AddHandler(
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