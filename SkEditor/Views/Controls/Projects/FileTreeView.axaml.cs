using Avalonia.Controls;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Views.Controls.Projects;

public partial class FileTreeView : UserControl
{
    public FileTreeView()
    {
        InitializeComponent();
    }

    public FileTreeView(FileTreeViewModel viewModel) : this() => DataContext = viewModel;
}