using ObservableCollections;
using R3;
using SkEditor.Models.Completions;

namespace SkEditor.ViewModels.Controls.Editor;

public class CompletionViewModel(ObservableList<CompletionData> completions)
{
    public required BindableReactiveProperty<CompletionData?> SelectedCompletion { get; init; }

    public NotifyCollectionChangedSynchronizedViewList<CompletionData> CompletionList { get; } =
        completions.ToNotifyCollectionChangedSlim();
}