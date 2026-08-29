using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features;

public class EditorOptionsFeature(AppSettings appSettings, IThemeService themes, INotificationService notifs)
    : IEditorFeature
{
    private DisposableBag _disposables;
    
    public bool CanAttach(IFeatureTab tab)
    {
        return tab is ITextEditorTab;
    }

    public void OnViewAttached(IFeatureTab tab, TextEditor editor)
    {
        _disposables = new DisposableBag();

        appSettings.ObserveAnyPropertyChanged()
            .AsUnitObservable()
            .Prepend(Unit.Default)
            .Subscribe(_ => UpdateOptions(editor))
            .AddTo(ref _disposables);
    }

    public void OnViewDetached(IFeatureTab tab, TextEditor editor)
    {
        _disposables.Dispose();
    }

    private void UpdateOptions(TextEditor editor)
    {
        editor.TextArea.TextView.LinkTextForegroundBrush = new ImmutableSolidColorBrush(Color.Parse("#1a94c4"));
        editor.TextArea.TextView.LinkTextUnderline = true;
        editor.TextArea.SelectionBrush = (ImmutableSolidColorBrush?)Application.Current?.FindResource("SelectionColor");
        editor.TextArea.RightClickMovesCaret = true;

        LineNumberMargin? lineNumberMargin = editor.TextArea.LeftMargins.OfType<LineNumberMargin>().FirstOrDefault();
        lineNumberMargin?.Margin = new Thickness(10, 0);
        lineNumberMargin?.MinWidthInDigits = 3;

        editor.Options.AllowScrollBelowDocument = true;
        editor.Options.CutCopyWholeLine = true;

        editor.Options.ConvertTabsToSpaces = !appSettings.UseEditorTabsForIndentation;
        editor.Options.IndentationSize = appSettings.EditorTabSize;

        editor.TextArea.TextView.CurrentLineBackground = themes.CurrentTheme.CurrentLineBackground;
        editor.TextArea.TextView.CurrentLineBorder = new ImmutablePen(themes.CurrentTheme.CurrentLineBorder, 2);
        editor.Options.HighlightCurrentLine = appSettings.HighlightCurrentLine;

        editor.Options.FixedLineHeight = true;
    }
}