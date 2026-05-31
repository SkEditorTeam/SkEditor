using Avalonia;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Behaviors;

public class TextEditorLoadBehavior : Behavior<TextEditor>
{
    private FileTabViewModel? _currentTab;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is not { } textEditor)
        {
            return;
        }

        textEditor.PropertyChanged += OnEditorPropertyChanged;

        if (textEditor.DataContext is FileTabViewModel tab)
        {
            UpdateTabBinding(null, tab);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject is not { } textEditor)
        {
            return;
        }

        textEditor.PropertyChanged -= OnEditorPropertyChanged;
        UpdateTabBinding(_currentTab, null);
    }

    private void OnEditorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == StyledElement.DataContextProperty)
        {
            UpdateTabBinding(e.OldValue as FileTabViewModel, e.NewValue as FileTabViewModel);
        }
    }

    private void UpdateTabBinding(FileTabViewModel? oldTab, FileTabViewModel? newTab)
    {
        if (ReferenceEquals(oldTab, newTab))
        {
            return;
        }

        IFileFeatureService featureService = App.Services.GetRequiredService<IFileFeatureService>();

        if (oldTab != null && AssociatedObject != null)
        {
            featureService.OnViewUnloaded(oldTab, AssociatedObject);
        }

        if (newTab != null && AssociatedObject != null)
        {
            featureService.OnViewLoaded(newTab, AssociatedObject);
        }

        _currentTab = newTab;

        if (newTab != null && AssociatedObject != null)
        {
            Dispatcher.UIThread.Post(
                () => AssociatedObject.TextArea.Focus(),
                DispatcherPriority.Loaded);
        }
    }
}