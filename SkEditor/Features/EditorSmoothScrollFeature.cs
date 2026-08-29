using System.Linq;
using Avalonia.Animation.Easings;
using Avalonia.Controls.Primitives;
using Avalonia.Xaml.Interactivity;
using AvaloniaEdit;
using R3;
using SkEditor.Behaviors;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features;

public class EditorSmoothScrollFeature(AppSettings appSettings) : IEditorFeature
{
    private DisposableBag _disposables;
    
    public bool CanAttach(IFeatureTab tab)
    {
        return appSettings.IsEditorSmoothScrollEnabled && tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _disposables = default;
        
        editor.TemplateApplied += OnEditorTemplateApplied;

        appSettings
            .ObservePropertyChanged(x => x.IsEditorSmoothScrollEnabled)
            .Subscribe(x =>
            {
                if (x)
                {
                    AddBehavior(editor);
                }
                else
                {
                    RemoveBehavior(editor);
                }
            })
            .AddTo(ref _disposables);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        editor.TemplateApplied -= OnEditorTemplateApplied;
        RemoveBehavior(editor);
        _disposables.Dispose();
    }

    private static void OnEditorTemplateApplied(object? sender, TemplateAppliedEventArgs e)
    {
        if (sender is not TextEditor editor)
        {
            return;
        }

        AddBehavior(editor);
    }

    private static void AddBehavior(TextEditor editor)
    {
        SmoothScrollBehavior behavior = new()
        {
            AnimationDuration = 250,
            ScrollEasing = new CubicEaseOut()
        };
        Interaction.SetBehaviors(editor.ScrollViewer, [behavior]);
    }

    private static void RemoveBehavior(TextEditor editor)
    {
        BehaviorCollection behaviors = Interaction.GetBehaviors(editor.ScrollViewer);
        SmoothScrollBehavior? smoothScrollBehavior = behaviors.OfType<SmoothScrollBehavior>().FirstOrDefault();
        if (smoothScrollBehavior != null)
        {
            behaviors.Remove(smoothScrollBehavior);
        }
    }
}