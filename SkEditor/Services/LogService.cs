using System;
using System.IO;
using R3;
using Serilog;
using Serilog.Core;
using SkEditor.Contracts.Services;
using SkEditor.Models.Logging;

#pragma warning disable CA2254

namespace SkEditor.Services;

public class LogService(ILogger logger) : ILogService, IDisposable
{
    private readonly Subject<LogEntry> _logSubject = new();

    public void Dispose()
    {
        _logSubject.OnCompleted();
        _logSubject.Dispose();
        GC.SuppressFinalize(this);
    }

    public string LogsFolderPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SkEditor", "v3.0", "Logs");

    public Observable<LogEntry> OnLogReceived => _logSubject;

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug(string messageTemplate, params object[] propertyValues)
    {
        logger.Debug(messageTemplate, propertyValues);
        Publish(LogLevel.Debug, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Info(string messageTemplate, params object[] propertyValues)
    {
        logger.Information(messageTemplate, propertyValues);
        Publish(LogLevel.Info, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning(string messageTemplate, params object[] propertyValues)
    {
        logger.Warning(messageTemplate, propertyValues);
        Publish(LogLevel.Warning, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(string messageTemplate, params object[] propertyValues)
    {
        logger.Error(messageTemplate, propertyValues);
        Publish(LogLevel.Error, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(
        Exception ex,
        string? messageTemplate,
        params object[] propertyValues)
    {
        messageTemplate ??= "An exception occurred: {ExceptionMessage}";

        logger.Error(ex, messageTemplate, propertyValues);
        Publish(LogLevel.Error, messageTemplate, propertyValues, ex);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
        logger.Fatal(messageTemplate, propertyValues);
        Publish(LogLevel.Fatal, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Fatal(
        Exception exception,
        string? messageTemplate = null,
        params object[] propertyValues)
    {
        messageTemplate ??= "An exception occurred: {ExceptionMessage}";

        logger.Fatal(exception, messageTemplate, propertyValues);
        Publish(LogLevel.Fatal, messageTemplate, propertyValues, exception);
    }

    private void Publish(
        LogLevel level,
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
}