using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using R3;
using SkEditor.Contracts.Services.Analyzer;
using SkEditor.Models.Analyzer;
using SkEditor.Utilities.Editor.TextMarkers;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features.Editing;

public class AnalyzerFeature(IAnalyzer analyzer, ExperimentsConfig experiments) : IEditorFeature
{
    private TextMarkerService? _textMarkers;
    private IDisposable? _textEnteredSubscription;

    public bool CanAttach(IFeatureTab tab)
    {
        if (!experiments.BottomDrawerEnabled || tab is not IFileTab fileTab)
        {
            return false;
        }

        string extension = Path.GetExtension(fileTab.Path ?? string.Empty).ToLowerInvariant();
        return extension == ".sk";
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _textEnteredSubscription = editor.TextChangedAsObservable()
            .Debounce(TimeSpan.FromSeconds(0.5))
            .SubscribeAwait(async (_, ct) => { await AnalyzeText(editor, ct); }, AwaitOperation.Switch);

        if (tab is FileTabViewModel fileTab)
        {
            _textMarkers = new TextMarkerService(fileTab);
            editor.TextArea.TextView.BackgroundRenderers.Add(_textMarkers);
            editor.TextArea.TextView.LineTransformers.Add(_textMarkers);
        }
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _textEnteredSubscription?.Dispose();
        _textEnteredSubscription = null;
    }

    private async Task AnalyzeText(TextEditor editor, CancellationToken ct)
    {
        AnalyzerResult? result = await analyzer.Analyze(editor.Text, ct);

        _textMarkers?.RemoveAll();

        if (result is null || _textMarkers is null)
        {
            return;
        }

        foreach (AnalyzerError analyzerError in result.Errors)
        {
            DocumentLine? line = editor.Document.GetLineByNumber(analyzerError.Line);
            (int length, int notEmptyColumn) = GetLineLengthAndNotEmptyColumn(editor, line);

            int offset = editor.Document.GetOffset(analyzerError.Line, notEmptyColumn);

            ITextMarker? marker = _textMarkers.Create(offset, length);
            marker?.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
            marker?.MarkerColor = analyzerError.Type switch
            {
                AnalyzerErrorType.Error => new Color(255, 237, 27, 60),
                AnalyzerErrorType.Warning => new Color(255, 252, 172, 0),
                AnalyzerErrorType.Info => new Color(255, 26, 144, 237),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }

    private static (int, int) GetLineLengthAndNotEmptyColumn(TextEditor textEditor, DocumentLine line)
    {
        int notEmptyColumn = 0;
        int length = line.Length;
        for (int i = 0; i < line.Length; i++)
        {
            if (char.IsWhiteSpace(textEditor.Document.GetCharAt(line.Offset + i)))
            {
                continue;
            }

            notEmptyColumn = i + 1;
            length -= i;
            break;
        }

        for (int i = line.Length - 1; i >= 0; i--)
        {
            if (char.IsWhiteSpace(textEditor.Document.GetCharAt(line.Offset + i)))
            {
                continue;
            }

            length -= line.Length - i - 1;
            break;
        }

        return (length, notEmptyColumn);
    }
}