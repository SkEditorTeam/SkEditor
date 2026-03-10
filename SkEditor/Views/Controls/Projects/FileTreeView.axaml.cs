using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using SkEditor.Models.Projects;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Views.Controls.Projects;

public partial class FileTreeView : UserControl
{
    public FileTreeView()
    {
        InitializeComponent();
        FileTree.AddHandler(DoubleTappedEvent, OnTreeViewDoubleTapped, RoutingStrategies.Bubble);
    }

    private static void OnTreeViewDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is not Control source)
            return;

        var treeViewItem = source.FindAncestorOfType<TreeViewItem>(includeSelf: true);
        if (treeViewItem?.DataContext is FileSystemNode { OpenCommand: not null } node &&
            node.OpenCommand.CanExecute(null))
        {
            node.OpenCommand.Execute(null);
        }
    }

    public FileTreeView(FileTreeViewModel viewModel) : this() => DataContext = viewModel;
}