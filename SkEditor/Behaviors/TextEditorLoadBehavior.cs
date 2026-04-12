using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Tabs;

namespace SkEditor.Behaviors;

public class TextEditorLoadBehavior : Behavior<TextEditor>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not { } textEditor)
        {
            return;
        }

        textEditor.Loaded += OnEditorLoaded;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();

        if (AssociatedObject is not { } textEditor)
        {
            return;
        }

        textEditor.Loaded -= OnEditorLoaded;
    }

    private void OnEditorLoaded(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextEditor { DataContext: FileTab tab } textEditor)
        {
            return;
        }

        App.Services.GetRequiredService<IFileFeatureService>().OnViewLoaded(tab, textEditor);
    }
}