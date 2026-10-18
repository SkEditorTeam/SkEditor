using Avalonia.Controls;

namespace SkEditor.Welcome.UI;

public partial class WhatsNewPage : UserControl
{
    public WhatsNewPage() => InitializeComponent();

    public WhatsNewPage(WhatsNewPageViewModel viewModel) : this() => DataContext = viewModel;
}