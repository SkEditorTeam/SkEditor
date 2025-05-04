using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using AvaloniaEdit.Document;
using AvaloniaEdit.Rendering;
using FluentAvalonia.UI.Windowing;
using Serilog.Events;
using LogEventLevel = Serilog.Events.LogEventLevel;

namespace SkEditor.Views;

public partial class LogsWindow : AppWindow
{
    public static readonly List<LogEvent> Logs = [];

    public LogsWindow()
    {
        InitializeComponent();
        Focusable = true;

        RenderDocument();

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };
    }

    public void RenderDocument()
    {
        LogsEditor.ShowLineNumbers = false;
        LogsEditor.Foreground = (ImmutableSolidColorBrush)Application.Current.FindResource("EditorTextColor");
        LogsEditor.Background = (ImmutableSolidColorBrush)Application.Current.FindResource("EditorBackgroundColor");
        LogsEditor.LineNumbersForeground =
            (ImmutableSolidColorBrush)Application.Current.FindResource("LineNumbersColor");
        LogsEditor.FontSize = 14;
        LogsEditor.WordWrap = true;
        LogsEditor.Margin = new Thickness(5);

        int lineNumber = 1;
        foreach (LogEvent logEvent in Logs)
        {
            LogsEditor.Document.Insert(LogsEditor.Document.TextLength, logEvent.RenderMessage());
            LogsEditor.Document.Insert(LogsEditor.Document.TextLength, "\n");

            Color color = logEvent.Level switch
            {
                LogEventLevel.Debug => Colors.Gray,
                LogEventLevel.Information => Colors.CornflowerBlue,
                LogEventLevel.Warning => Colors.Orange,
                LogEventLevel.Error => Colors.OrangeRed,
                LogEventLevel.Fatal => Colors.Red,
                _ => Colors.Bisque
            };

            LogsEditor.TextArea.TextView.LineTransformers.Add(new LineColorizer(lineNumber, color));
            lineNumber++;
        }
    }

    public class LineColorizer(int lineNumber, Color color) : DocumentColorizingTransformer
    {
        protected override void ColorizeLine(DocumentLine line)
        {
            if (!line.IsDeleted && line.LineNumber == lineNumber)
            {
                ChangeLinePart(line.Offset, line.EndOffset, ApplyChanges);
            }
        }

        private void ApplyChanges(VisualLineElement element)
        {
            element.TextRunProperties.SetForegroundBrush(new SolidColorBrush(color));
        }
    }
}