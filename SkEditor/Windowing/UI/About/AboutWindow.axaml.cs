namespace SkEditor.Windowing.UI.About;

public partial class AboutWindow : SkEditorWindow
{
    public AboutWindow() => InitializeComponent();

    public AboutWindow(AboutWindowViewModel viewModel) : this() => DataContext = viewModel;
}