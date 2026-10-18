using System;
using System.Text;

namespace SkEditor.Core.Logging;

public record LogEntry(
    LogLevel Level,
    LogSource Source,
    string Message,
    DateTime Timestamp,
    Exception? Exception = null)
{
    public override string ToString()
    {
        StringBuilder sb = new();
        sb.Append($"[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level}] {Message}");
        if (Exception == null)
        {
            return sb.ToString();
        }

        sb.AppendLine();
        sb.AppendLine(Exception.ToString());
        return sb.ToString();
    }
}