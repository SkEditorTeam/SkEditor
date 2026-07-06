using System;
using System.Threading;
using System.Threading.Tasks;
using AvaloniaEdit;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Analyzer;
using SkEditor.Models.Analyzer;
using SkEditor.Utilities.Extensions;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features.Editing;

public class AnalyzerFeature(IAnalyzer analyzer, INotificationService notifs) : IEditorFeature
{
    private IDisposable? _textEnteredSubscription;

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _textEnteredSubscription = editor.TextChangedAsObservable()
            .Debounce(TimeSpan.FromSeconds(0.5))
            .SubscribeAwait(async (_, ct) => { await AnalyzeText(editor.Text, ct); }, AwaitOperation.Switch);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _textEnteredSubscription?.Dispose();
        _textEnteredSubscription = null;
    }

    private async Task AnalyzeText(string text, CancellationToken ct)
    {
        AnalyzerResult? result = await analyzer.Analyze(text, ct);
    }
}