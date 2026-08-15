using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Input;
using AvaloniaEdit;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using R3;
using SkEditor.Models.Analyzer;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Settings;
using SkEditor.ViewModels.Tabs.Capabilities;
using SkEditor.Views.Controls.Editor;

namespace SkEditor.Features.Analyzer;

public class AnalyzerTooltipFeature(ExperimentsConfig experiments)
    : IEditorFeature
{
    private static readonly ErrorResultComparer Comparer = new();
    private static readonly TimeSpan HideGracePeriod = TimeSpan.FromMilliseconds(100);
    private readonly TimeSpan _tooltipDelay = TimeSpan.FromMilliseconds(500);

    private DisposableBag _disposableBag;
    private Point _lastPointerPosition;
    private bool _pointerOverPopup;

    private Popup? _popup;
    private bool _tooltipShown;
    private AnalyzerTooltipView? _tooltipView;

    public bool CanAttach(IFeatureTab tab)
    {
        if (!experiments.AnalyzerEnabled || !experiments.AnalyzerErrorTooltipsEnabled || tab is not IFileTab fileTab ||
            tab is not IDiagnosticsTab)
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

        _disposableBag = new DisposableBag();

        _tooltipView = new AnalyzerTooltipView();
        _popup = new Popup
        {
            Topmost = false,
            PlacementTarget = editor.TextArea.TextView,
            Placement = PlacementMode.AnchorAndGravity,
            PlacementAnchor = PopupAnchor.BottomLeft,
            PlacementGravity = PopupGravity.BottomRight,
            PlacementConstraintAdjustment = PopupPositionerConstraintAdjustment.SlideX,
            HorizontalOffset = -2,
            Child = _tooltipView
        };

        Observable.FromEventHandler<PointerEventArgs>(
                h => _tooltipView.PointerEntered += h,
                h => _tooltipView.PointerEntered -= h
            )
            .Subscribe(_ => _pointerOverPopup = true)
            .AddTo(ref _disposableBag);

        Observable.FromEventHandler<PointerEventArgs>(
                h => _tooltipView.PointerExited += h,
                h => _tooltipView.PointerExited -= h
            )
            .Subscribe(_ =>
            {
                _pointerOverPopup = false;
                _popup?.IsOpen = false;
                _tooltipShown = false;
            })
            .AddTo(ref _disposableBag);

        ((ISetLogicalParent)_popup).SetParent(editor.TextArea.TextView);

        editor.PointerMovedAsObservable()
            .ThrottleLast(TimeSpan.FromMilliseconds(50))
            .Subscribe(e => _lastPointerPosition = e.GetPosition(editor.TextArea.TextView))
            .AddTo(ref _disposableBag);

        Observable<(AnalyzerError? Error, Rect TargetRect)> movedStream = editor.PointerMovedAsObservable()
            .Select(e =>
            {
                Point point = e.GetPosition(editor.TextArea.TextView);
                return GetErrorAndTargetRect(point, editor, diagnosticsTab);
            });

        Observable<(AnalyzerError?, Rect)> exitedStream = editor.PointerExitedAsObservable()
            .Select(_ => ((AnalyzerError?)null, default(Rect)));

        Observable.Merge(movedStream, exitedStream)
            .DistinctUntilChanged(Comparer)
            .SubscribeAwait(async (result, ct) => { await HandlePointerMoved(editor, result, diagnosticsTab, ct); },
                AwaitOperation.Switch)
            .AddTo(ref _disposableBag);

        Observable.FromEventHandler(
                h => editor.TextArea.TextView.ScrollOffsetChanged += h,
                h => editor.TextArea.TextView.ScrollOffsetChanged -= h
            )
            .Subscribe(_ =>
            {
                _pointerOverPopup = false;
                _popup?.IsOpen = false;
                _tooltipShown = false;
            })
            .AddTo(ref _disposableBag);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _disposableBag.Dispose();

        if (_popup is not null)
        {
            _popup.IsOpen = false;
            _popup.Child = null;
        }

        _tooltipShown = false;
        _pointerOverPopup = false;
        _lastPointerPosition = default;

        _popup = null;
        _tooltipView = null;
    }

    private async Task HandlePointerMoved(TextEditor editor, (AnalyzerError? Error, Rect TargetRect) result,
        IDiagnosticsTab diagnosticsTab, CancellationToken ct)
    {
        (AnalyzerError? error, _) = result;

        if (error is null)
        {
            if (!_tooltipShown)
            {
                return;
            }

            await Task.Delay(HideGracePeriod, ct);

            if (_pointerOverPopup)
            {
                return;
            }

            _popup?.IsOpen = false;
            _tooltipShown = false;
            return;
        }

        await Task.Delay(_tooltipDelay, ct);

        (AnalyzerError? currentError, Rect currentTargetRect) =
            GetErrorAndTargetRect(_lastPointerPosition, editor, diagnosticsTab);

        if (currentError is null || !Equals(currentError, error))
        {
            return;
        }

        _tooltipView?.Diagnostic = error;
        _popup?.PlacementRect = currentTargetRect;
        _popup?.IsOpen = true;

        _tooltipShown = true;
    }

    private static (AnalyzerError? Error, Rect TargetRect) GetErrorAndTargetRect(Point point, TextEditor editor,
        IDiagnosticsTab diagnosticsTab)
    {
        TextView? textView = editor.TextArea.TextView;

        Point visualPoint = point + textView.ScrollOffset;

        int lineNumber = textView.GetPosition(visualPoint)?.Line ?? -1;
        VisualLine? visualLine = textView.GetVisualLine(lineNumber);

        if (visualLine is null)
        {
            return (null, default);
        }

        TextViewPosition visualPos = visualLine.GetTextViewPosition(visualPoint, true);

        DocumentLine? line = editor.Document.GetLineByNumber(lineNumber);
        (int length, int notEmptyColumn) = editor.GetLineLengthAndNotEmptyColumn(line);

        int hoverColumn = visualPos.VisualColumn > -1
            ? visualPos.VisualColumn
            : visualPos.Column;

        int startOffset = notEmptyColumn - 1;
        int endOffset = startOffset + length;

        if (hoverColumn < startOffset || hoverColumn > endOffset)
        {
            return (null, default);
        }

        diagnosticsTab.DiagnosticDictionary.TryGetValue(lineNumber, out AnalyzerError? errorAtPos);

        double lineBottomY = visualLine.VisualTop + visualLine.Height - textView.ScrollOffset.Y;
        Rect targetRect = new(point.X, lineBottomY, 1, 1);

        return (errorAtPos, targetRect);
    }

    private class ErrorResultComparer : IEqualityComparer<(AnalyzerError? Error, Rect TargetRect)>
    {
        public bool Equals((AnalyzerError? Error, Rect TargetRect) x, (AnalyzerError? Error, Rect TargetRect) y)
        {
            return Equals(x.Error, y.Error);
        }

        public int GetHashCode((AnalyzerError? Error, Rect TargetRect) obj)
        {
            return obj.Error?.GetHashCode() ?? 0;
        }
    }
}