using SkEditor.Windowing.UI;

namespace SkEditor.Integrations.Publishing.UI;

public partial class PublishWindow : SkEditorWindow
{
    public PublishWindow() => InitializeComponent();

    public PublishWindow(PublishWindowViewModel viewModel) : this() => DataContext = viewModel;
}