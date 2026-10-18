using ObservableCollections;
using R3;
using SkEditor.Editor.Completions.Models;

namespace SkEditor.Editor.Completions;

public class CompletionViewModel(ObservableList<CompletionData> completions)
{
    public required BindableReactiveProperty<CompletionData?> SelectedCompletion { get; init; }

    public NotifyCollectionChangedSynchronizedViewList<CompletionData> CompletionList { get; } =
        completions.ToNotifyCollectionChangedSlim();
}