using Avalonia.Controls;

namespace SkEditor.Layout.StatusBar.UI;

public partial class StatusBarControl : UserControl
{
    public StatusBarControl()
    {
        InitializeComponent();

        DataContextChanged += (_, _) =>
        {
            if (DataContext is StatusBarViewModel vm)
            {
                vm.RequestFlyoutHide += () =>
                {
                    NotificationButton.Flyout?.Hide();
                };
            }
        };
    }
}