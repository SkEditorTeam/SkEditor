using SkEditor.ViewModels.Windows;
using SkEditor.Views.Controls;

namespace SkEditor.Views.Windows;

public partial class AboutWindow : SkEditorWindow
{
    public AboutWindow() => InitializeComponent();

    public AboutWindow(AboutWindowViewModel viewModel) : this() => DataContext = viewModel;
}