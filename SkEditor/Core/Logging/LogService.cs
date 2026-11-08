using System;
using R3;
using Serilog;
using Serilog.Core;
using SkEditor.Core.Common;

#pragma warning disable CA2254

namespace SkEditor.Core.Logging;

public class LogService(ILogger logger) : ILogService, IDisposable
{
    private readonly Subject<LogEntry> _logSubject = new();

    public string LogsFolderPath => AppPaths.Logs;

    public Observable<LogEntry> OnLogReceived => _logSubject;

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues)
    {
        logger.ForContext("Source", source).Debug(messageTemplate, propertyValues);
        Publish(LogLevel.Debug, source, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Info(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues)
    {
        logger.ForContext("Source", source).Information(messageTemplate, propertyValues);
        Publish(LogLevel.Info, source, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues)
    {
        logger.ForContext("Source", source).Warning(messageTemplate, propertyValues);
        Publish(LogLevel.Warning, source, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues)
    {
        logger.ForContext("Source", source).Error(messageTemplate, propertyValues);
        Publish(LogLevel.Error, source, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(
        Exception ex,
        string? messageTemplate = null,
        LogSource source = LogSource.SkEditor,
        params object[] propertyValues)
    {
        messageTemplate ??= ex.Message;

        logger.ForContext("Source", source).Error(ex, messageTemplate, propertyValues);
        Publish(LogLevel.Error, source, messageTemplate, propertyValues, ex);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues)
    {
        logger.ForContext("Source", source).Fatal(messageTemplate, propertyValues);
        Publish(LogLevel.Fatal, source, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal(
        Exception exception,
        string? messageTemplate = null,
        LogSource source = LogSource.SkEditor,
        params object[] propertyValues)
    {
        messageTemplate ??= exception.Message;
        
        logger.ForContext("Source", source).Fatal(exception, messageTemplate, propertyValues);
        Publish(LogLevel.Fatal, source, messageTemplate, propertyValues, exception);
    }

    private void Publish(
        LogLevel level,
        LogSource source,
        string template,
        object[] values,
        Exception? ex = null)
    {
        string formattedMessage = values.Length > 0
            ? FormatTemplate(template, values)
            : template;

        _logSubject.OnNext(
            new LogEntry(
                level,
                source,
                formattedMessage,
                DateTime.Now,
                ex));
    }

    private static string FormatTemplate(string template, object[] values)
    {
        try
        {
            return string.Format(template, values);
        }
        catch (FormatException)
        {
            return template;
        }
    }

    public void Dispose()
    {
        _logSubject.OnCompleted();
        _logSubject.Dispose();
        GC.SuppressFinalize(this);
    }
}