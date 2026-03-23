using System;
using Serilog.Core;
using Serilog.Events;
using LogsWindow = SkEditor.Views.Windows.LogsWindow;

namespace SkEditor.Utilities;

public class LogsHandler : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        LogsWindow.Logs.Add(logEvent);

        string color = GetColor(logEvent.Level);
        if (logEvent.Level == LogEventLevel.Fatal)
        {
            Console.WriteLine(color + logEvent.Level + " | " + logEvent.RenderMessage());
            Console.WriteLine(color + logEvent.Exception);
        }
        else
        {
            Console.WriteLine(color + logEvent.Level + " | " + logEvent.RenderMessage());
        }
    }

    private static string GetColor(LogEventLevel level)
    {
        return level switch
        {
            LogEventLevel.Debug => "\e[37m",
            LogEventLevel.Information => "\e[32m",
            LogEventLevel.Warning => "\e[33m",
            LogEventLevel.Error => "\e[31m",
            LogEventLevel.Fatal => "\e[31m",
            _ => "\e[37m"
        };
    }
}