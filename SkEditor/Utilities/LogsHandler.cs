using System;
using Serilog.Core;
using Serilog.Events;
using SkEditor.API;

namespace SkEditor.Utilities;

public class LogsHandler : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        if (logEvent.Level == LogEventLevel.Debug)
            SkEditorAPI.Windows.GetMainWindow().BottomBar.UpdateLogs(logEvent.RenderMessage());
        
        Console.WriteLine(logEvent.RenderMessage());
    }
}