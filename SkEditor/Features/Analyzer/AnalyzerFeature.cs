using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using R3;
using SkEditor.Analyzing;
using SkEditor.Models.Analyzer;
using SkEditor.Utilities.Editor.TextMarkers;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features.Analyzer;

public class AnalyzerFeature(IAnalyzer analyzer, ExperimentsConfig experiments) : IEditorFeature
{
    private CancellationTokenSource? _featureCts;
    private IDisposable? _textEnteredSubscription;
    private TextMarkerService? _textMarkers;
    
    private readonly Dictionary<DocumentLine, ITextMarker> _textMarkersDict = new();

    public bool CanAttach(IFeatureTab tab)
    {
        if (!experiments.AnalyzerEnabled || tab is not IFileTab fileTab || tab is not IDiagnosticsTab)
        {
            return false;
        }

        return fileTab.IsAnalyzable;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        if (tab is not IDiagnosticsTab diagnosticsTab)
        {
            return;
        }

        _featureCts = new CancellationTokenSource();

        _textEnteredSubscription = editor.TextChangedAsObservable()
            .Debounce(TimeSpan.FromSeconds(0.5))
            .SubscribeAwait(async (_, ct) => { await AnalyzeText(diagnosticsTab, editor, ct); }, AwaitOperation.Switch);

        if (tab is FileTabViewModel fileTab)
        {
            _textMarkers = new TextMarkerService(fileTab);
            editor.TextArea.TextView.BackgroundRenderers.Add(_textMarkers);
            editor.TextArea.TextView.LineTransformers.Add(_textMarkers);
        }

        _ = AnalyzeText(diagnosticsTab, editor, _featureCts.Token);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _featureCts?.Cancel();
        _featureCts?.Dispose();
        _featureCts = null;

        if (tab is IDiagnosticsTab diagnosticsTab)
        {
            diagnosticsTab.Diagnostics.Clear();
            editor.TextArea.TextView.BackgroundRenderers.Remove(_textMarkers);
            editor.TextArea.TextView.LineTransformers.Remove(_textMarkers);
        }

        _textEnteredSubscription?.Dispose();
        _textEnteredSubscription = null;
    }

    private async Task AnalyzeText(IDiagnosticsTab tab, TextEditor editor, CancellationToken ct)
    {
        AnalyzerResult? result = await analyzer.Analyze(editor.Text, ct);

        if (ct.IsCancellationRequested)
        {
            return;
        }

        await UpdateTabDiagnostics(tab, result);

        if (result is null || _textMarkers is null)
        {
            _textMarkersDict.Clear();
            _textMarkers?.RemoveAll();
            return;
        }
        
        foreach ((_, ITextMarker textMarker) in _textMarkersDict)
        {
            _textMarkers.Remove(textMarker);
        }
        
        foreach (AnalyzerError analyzerError in result.Errors)
        {
            DocumentLine? line = editor.Document.GetLineByNumber(analyzerError.Line);
            (int length, int notEmptyColumn) = editor.GetLineLengthAndNotEmptyColumn(line);

            int offset = editor.Document.GetOffset(analyzerError.Line, notEmptyColumn);

            if (_textMarkersDict.TryGetValue(line, out ITextMarker? existingMarker))
            {
                _textMarkers.Remove(existingMarker);
            }

            ITextMarker? marker = _textMarkers.Create(offset, length);
            marker?.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
            marker?.MarkerColor = analyzerError.Type switch
            {
                AnalyzerErrorType.Error => new Color(255, 237, 27, 60),
                AnalyzerErrorType.Warning => new Color(255, 252, 172, 0),
                AnalyzerErrorType.Info => new Color(255, 26, 144, 237),
                _ => throw new ArgumentOutOfRangeException()
            };

            if (marker is not null)
            {
                _textMarkersDict[line] = marker;
            }
        }
    }

    private static async Task UpdateTabDiagnostics(IDiagnosticsTab tab, AnalyzerResult? result)
    {
        tab.DiagnosticDictionary.Clear();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            tab.Diagnostics.Clear();

            if (result is null)
            {
                return;
            }

            tab.Diagnostics.AddRange(result.Errors);
        });

        if (result is not null)
        {
            Dictionary<int, AnalyzerError> newErrorsDict = result.Errors
                .GroupBy(e => e.Line)
                .ToDictionary(g => g.Key, g => g.First());

            foreach ((int line, AnalyzerError error) in newErrorsDict)
            {
                tab.DiagnosticDictionary[line] = error;
            }
        }
    }
}