using SkEditor.ViewModels.Windows;
using SkEditor.Views.Controls;

namespace SkEditor.Views.Windows;

public partial class PublishWindow : SkEditorWindow
{
    public PublishWindow() => InitializeComponent();

    public PublishWindow(PublishWindowViewModel viewModel) : this() => DataContext = viewModel;
}