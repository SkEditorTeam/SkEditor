using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using SkEditor.Behaviors;
using SkEditor.Models.Projects.Types;
using SkEditor.ViewModels.Controls.FileTree;

namespace SkEditor.Views.Controls.Projects;

public partial class FileTreeView : UserControl
{
    public FileTreeView()
    {
        InitializeComponent();
        FileTree.AddHandler(TappedEvent, OnTreeViewSingleTapped, RoutingStrategies.Bubble);
        FileTree.AddHandler(DoubleTappedEvent, OnTreeViewDoubleTapped, RoutingStrategies.Bubble);
        FileTree.AddHandler(ContextRequestedEvent, OnContextRequested, RoutingStrategies.Tunnel);

        FileTree.TemplateApplied += OnFileTreeTemplateApplied;
    }

    public FileTreeView(FileTreeViewModel viewModel) : this() => DataContext = viewModel;

    private void OnFileTreeTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        ScrollViewer? scrollViewer = FileTree.GetTemplateDescendants()
            .OfType<ScrollViewer>()
            .FirstOrDefault();
        if (scrollViewer is null)
        {
            return;
        }

        BehaviorCollection collection = [new VerticalScrollViewerAnimatedBehavior()];
        Interaction.SetBehaviors(scrollViewer, collection);
    }

    private void OnTreeViewSingleTapped(object? sender, TappedEventArgs e)
    {
        CheckTap(e, false);
    }

    private void OnTreeViewDoubleTapped(object? sender, TappedEventArgs e)
    {
        CheckTap(e, true);
    }

    private void CheckTap(TappedEventArgs e, bool isDoubleTap)
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
        if (DataContext is FileTreeViewModel vm &&
            treeViewItem?.DataContext is FileSystemNode { OpenCommand: not null } node)
        {
            vm.HandleNodeClick(node, isDoubleTap);
        }
    }

    private static void OnContextRequested(object? sender, ContextRequestedEventArgs e)
    {
        if (e.Source is not Control source)
        {
            return;
        }

        TreeViewItem? treeViewItem = source.FindAncestorOfType<TreeViewItem>(true);
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