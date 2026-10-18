using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using AvaloniaEdit.Utils;
using R3;
using SkEditor.Core.Logging;
using SkEditor.Layout.StatusBar.UI.Panes.Logs.Converters;

namespace SkEditor.Layout.StatusBar.UI.Panes.Logs;

public static class LogInlinesBehavior
{
    public static readonly AttachedProperty<IEnumerable<LogEntry>?> LogsProperty =
        AvaloniaProperty.RegisterAttached<SelectableTextBlock, IEnumerable<LogEntry>?>(
            "Logs", typeof(LogInlinesBehavior));

    private static readonly LogLevelColorConverter LevelColorConverter = new();
    private static readonly LogLevelMessageColorConverter MessageColorConverter = new();
    private static readonly LogLevelSymbolConverter SymbolConverter = new();

    private static readonly AttachedProperty<IDisposable?> SubscriptionProperty =
        AvaloniaProperty.RegisterAttached<SelectableTextBlock, IDisposable?>(
            "Subscription", typeof(LogInlinesBehavior));

    static LogInlinesBehavior() => LogsProperty.Changed.Subscribe(OnLogsChanged);

    public static void SetLogs(SelectableTextBlock element, IEnumerable<LogEntry>? value)
    {
        element.SetValue(LogsProperty, value);
    }

    public static IEnumerable<LogEntry>? GetLogs(SelectableTextBlock element)
    {
        return element.GetValue(LogsProperty);
    }

    private static void OnLogsChanged(AvaloniaPropertyChangedEventArgs<IEnumerable<LogEntry>?> e)
    {
        if (e.Sender is not SelectableTextBlock textBlock)
        {
            return;
        }

        IDisposable? currentSubscription = textBlock.GetValue(SubscriptionProperty);
        currentSubscription?.Dispose();

        IEnumerable<LogEntry>? logs = e.NewValue.Value;

        RebuildInlines(textBlock, logs);

        if (logs is INotifyCollectionChanged notifyCollection)
        {
            IDisposable subscription = Observable
                .FromEvent<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                    h => (_, args) => h(args),
                    h => notifyCollection.CollectionChanged += h,
                    h => notifyCollection.CollectionChanged -= h
                )
                .Subscribe(_ => RebuildInlines(textBlock, logs));

            textBlock.SetValue(SubscriptionProperty, subscription);
        }
        else
        {
            textBlock.SetValue(SubscriptionProperty, null);
        }
    }

    private static void RebuildInlines(SelectableTextBlock textBlock, IEnumerable<LogEntry>? logs)
    {
        InlineCollection inlines = new();

        if (logs != null)
        {
            int i = 0;
            foreach (LogEntry log in logs)
            {
                if (i++ > 0)
                {
                    inlines.Add(new LineBreak());
                }

                inlines.Add(new Run("["));

                inlines.Add(new Run(log.Timestamp.ToString("HH:mm:ss")));

                string symbol =
                    SymbolConverter.Convert(log.Level, typeof(string), null, CultureInfo.InvariantCulture) as string ??
                    log.Level.ToString();
                IBrush? levelBrush =
                    LevelColorConverter.Convert(log.Level, typeof(IBrush), null,
                        CultureInfo.InvariantCulture) as IBrush;

                Run levelRun = new($" {symbol}")
                {
                    Foreground = levelBrush
                };
                inlines.Add(levelRun);

                inlines.Add(new Run("] "));

                IBrush? messageBrush =
                    MessageColorConverter.Convert(log.Level, typeof(IBrush), null, CultureInfo.InvariantCulture) as
                        IBrush;
                Run messageRun = new(log.Message)
                {
                    Foreground = messageBrush
                };
                inlines.Add(messageRun);
            }
        }

        textBlock.Inlines = inlines;
    }
}