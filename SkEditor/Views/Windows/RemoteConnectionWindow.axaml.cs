using FluentAvalonia.UI.Windowing;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class RemoteConnectionWindow : AppWindow
{
    public RemoteConnectionWindow()
    {
        InitializeComponent();
        Focusable = true;
        
        if (DataContext is RemoteConnectionViewModel vm)
            vm.CloseRequested += (_, _) => Close();
    }
}