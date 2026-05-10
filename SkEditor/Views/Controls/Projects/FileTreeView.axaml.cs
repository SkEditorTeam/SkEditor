using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using SkEditor.Models.Projects.Types;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Views.Controls.Projects;

public partial class FileTreeView : UserControl
{
    public FileTreeView()
    {
        InitializeComponent();
        FileTree.AddHandler(DoubleTappedEvent, OnTreeViewDoubleTapped, RoutingStrategies.Bubble);
        FileTree.AddHandler(ContextRequestedEvent, OnContextRequested, RoutingStrategies.Tunnel);
    }

    public FileTreeView(FileTreeViewModel viewModel) : this() => DataContext = viewModel;

    private static void OnTreeViewDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (e.Source is not Control source)
        {
            return;
        }

        // ignore double clicking on the folder chevron
        if (source.FindAncestorOfType<ToggleButton>(true)?.Name == "PART_ExpandCollapseChevron")
        {
            return;
        }

        TreeViewItem? treeViewItem = source.FindAncestorOfType<TreeViewItem>(true);
        if (treeViewItem?.DataContext is FileSystemNode { OpenCommand: not null } node &&
            node.OpenCommand.CanExecute(null))
        {
            node.OpenCommand.Execute(null);
        }
    }

    private static void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (e.Source is not Control source)
        {
            return;
        }

        var treeViewItem = source.FindAncestorOfType<TreeViewItem>(true);
        if (treeViewItem is null)
        {
            e.Handled = true;
        }
        else
        {
            treeViewItem.Focus();
        }
    }
}