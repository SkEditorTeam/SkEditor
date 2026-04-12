using Avalonia;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;

namespace SkEditor.Behaviors;

public class TextEditorLoadBehavior : Behavior<TextEditor>
{
    private FileTab? _currentTab;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject is not { } textEditor) return;

        textEditor.PropertyChanged += OnEditorPropertyChanged;
        
        if (textEditor.DataContext is FileTab tab)
        {
            UpdateTabBinding(null, tab);
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject is not { } textEditor) return;

        textEditor.PropertyChanged -= OnEditorPropertyChanged;
        UpdateTabBinding(_currentTab, null);
    }

    private void OnEditorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == StyledElement.DataContextProperty)
        {
            UpdateTabBinding(e.OldValue as FileTab, e.NewValue as FileTab);
        }
    }

    private void UpdateTabBinding(FileTab? oldTab, FileTab? newTab)
    {
        if (ReferenceEquals(oldTab, newTab)) return;

        var featureService = App.Services.GetRequiredService<IFileFeatureService>();

        if (oldTab != null && AssociatedObject != null)
        {
            featureService.OnViewUnloaded(oldTab, AssociatedObject);
        }

        if (newTab != null && AssociatedObject != null)
        {
            featureService.OnViewLoaded(newTab, AssociatedObject);
        }

        _currentTab = newTab;
    }
}