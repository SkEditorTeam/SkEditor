using Avalonia.Controls;
using Avalonia.Interactivity;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Views.Pages;

public partial class FileTabView : UserControl
{
    public FileTabView()
    {
        InitializeComponent();

        Editor.AddHandler(
            GotFocusEvent,
            (_, _) => (DataContext as FileTabViewModel)?.FocusCommand.Execute(null),
            RoutingStrategies.Bubble,
            true);
    }
}