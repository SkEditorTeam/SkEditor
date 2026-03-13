using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using SkEditor.Models.Projects;

namespace SkEditor.Behaviors;

public class FileTreeRenameBehavior : Behavior<TextBox>
{
    private string _originalName = string.Empty;

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject?.PropertyChanged += OnIsVisibleChanged;
        AssociatedObject?.KeyDown += OnKeyDown;
        AssociatedObject?.LostFocus += OnLostFocus;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject?.PropertyChanged -= OnIsVisibleChanged;
        AssociatedObject?.KeyDown -= OnKeyDown;
        AssociatedObject?.LostFocus -= OnLostFocus;
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

        Dispatcher.UIThread.Post(() =>
        {
            AssociatedObject.Focus();
            AssociatedObject.CaretIndex = AssociatedObject.Text?.Length ?? 0;
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

        node.IsRenaming = false;
        ReturnFocus();
    }

    private void Cancel()
    {
        if (AssociatedObject?.DataContext is not FileSystemNode node)
        {
            return;
        }

        node.Name = _originalName;
        node.IsRenaming = false;
        ReturnFocus();
    }

    private void ReturnFocus()
    {
        TreeViewItem? treeViewItem = AssociatedObject.FindAncestorOfType<TreeViewItem>();
        treeViewItem?.Focus();
    }
}