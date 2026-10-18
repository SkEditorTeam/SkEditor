using Avalonia.Controls;
using SkEditor.ViewModels.Pages;

namespace SkEditor.Views.Pages;

public partial class WhatsNewPage : UserControl
{
    public WhatsNewPage() => InitializeComponent();

    public WhatsNewPage(WhatsNewPageViewModel viewModel) : this() => DataContext = viewModel;
}