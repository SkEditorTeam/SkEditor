using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using SkEditor.Behaviors;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Features;

public class EditorSmoothScrollFeature(AppSettings appSettings) : IEditorFeature
{
    public bool CanAttach(IFeatureTab tab)
    {
        return appSettings.IsEditorSmoothScrollEnabled;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        editor.TemplateApplied += OnEditorTemplateApplied;
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        editor.TemplateApplied -= OnEditorTemplateApplied;
    }

    private static void OnEditorTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is not TextEditor editor)
        {
            return;
        }

        VerticalScrollViewerAnimatedBehavior behavior = new()
        {
            AnimationDuration = 250,
            ScrollEasing = new CubicEaseOut()
        };
        Interaction.SetBehaviors(editor.ScrollViewer, [behavior]);
    }
}