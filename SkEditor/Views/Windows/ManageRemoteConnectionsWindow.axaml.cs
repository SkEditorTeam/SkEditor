using SkEditor.ViewModels.Windows;
using SkEditor.Views.Controls;

namespace SkEditor.Views.Windows;

public partial class ManageRemoteConnectionsWindow : SkEditorWindow
{
    public ManageRemoteConnectionsWindow() => InitializeComponent();

    public ManageRemoteConnectionsWindow(RemoteConnectionViewModel viewModel) : this()
    {
        DataContext = viewModel;
        SetupViewModelEvents(viewModel);
    }

    private void SetupViewModelEvents(RemoteConnectionViewModel vm)
    {
        vm.PasteCompleted += () => HostTextBox.CaretIndex = HostTextBox.Text?.Length ?? 0;
    }
}