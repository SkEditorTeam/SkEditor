using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using SkEditor.Models.Projects.Types;

namespace SkEditor.Behaviors;

public class FileTreeRenameBehavior : Behavior<TextBox>
{
    private string _originalName = string.Empty;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.PropertyChanged += OnIsVisibleChanged;
        AssociatedObject?.LostFocus += OnLostFocus;

        AssociatedObject?.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        AssociatedObject?.AddHandler(InputElement.KeyDownEvent, OnArrowKeyDown, RoutingStrategies.Bubble);
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.PropertyChanged -= OnIsVisibleChanged;
        AssociatedObject?.LostFocus -= OnLostFocus;
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        AssociatedObject?.RemoveHandler(InputElement.KeyDownEvent, OnArrowKeyDown);
    }

    private void OnIsVisibleChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != Visual.IsVisibleProperty)
        {
            return;
        }

        if (e.NewValue is not true || AssociatedObject?.DataContext is not FileSystemNode node)
        {
            return;
        }

        _originalName = node.Name;
        AssociatedObject.Text = node.Name;

        Dispatcher.UIThread.Post(() =>
        {
            AssociatedObject.Focus();
            if (node.IsDirectory || node.Name.LastIndexOf('.') == -1)
            {
                AssociatedObject.SelectAll();
            }
            else
            {
                int extensionIndex = node.Name.LastIndexOf('.');
                AssociatedObject.SelectionStart = 0;
                AssociatedObject.SelectionEnd = extensionIndex;
            }
        });
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Enter:
            case Key.F2:
                e.Handled = true;
                Confirm();
                break;
            case Key.Escape:
                e.Handled = true;
                Cancel();
                break;
        }
    }

    private void OnArrowKeyDown(object? sender, KeyEventArgs e)
    {
        e.Handled = e.Key switch
        {
            Key.Left or Key.Right or Key.Up or Key.Down or Key.Home or Key.End => true,
            _ => e.Handled
        };
    }

    private void OnLostFocus(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject?.DataContext is FileSystemNode { IsRenaming: true })
        {
            Confirm();
        }
    }

    private void Confirm()
    {
        if (AssociatedObject?.DataContext is not FileSystemNode node)
        {
            return;
        }

        node.OnRenamed(_originalName, AssociatedObject.Text ?? _originalName);
        ReturnFocus();
    }

    private void Cancel()
    {
        if (AssociatedObject?.DataContext is not FileSystemNode)
        {
            return;
        }

        ReturnFocus();
    }

    private void ReturnFocus()
    {
        TreeViewItem? treeViewItem = AssociatedObject.FindAncestorOfType<TreeViewItem>();
        treeViewItem?.Focus();
    }
}