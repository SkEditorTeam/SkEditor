using System;
using Serilog;
using Serilog.Core;
using SkEditor.Contracts.Services;

#pragma warning disable CA2254

namespace SkEditor.Services;

public class LogService(ILogger logger) : ILogService
{
    [MessageTemplateFormatMethod("messageTemplate")]
    public void Debug(string messageTemplate, params object[] propertyValues)
    {
        logger.Debug(messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Info(string messageTemplate, params object[] propertyValues)
    {
        logger.Information(messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Warning(string messageTemplate, params object[] propertyValues)
    {
        logger.Warning(messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(string messageTemplate, params object[] propertyValues)
    {
        logger.Error(messageTemplate, propertyValues);
    }

    public void Fatal(string messageTemplate, params object[] propertyValues)
    {
        logger.Fatal(messageTemplate, propertyValues);
    }

    public void Fatal(Exception exception, string? messageTemplate = null, params object[] propertyValues)
    {
        messageTemplate ??= "An exception occurred: {ExceptionMessage}";
        logger.Fatal(exception, messageTemplate, propertyValues);
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(bool informUser, string messageTemplate, params object[] propertyValues)
    {
        throw new NotImplementedException();
    }

    [MessageTemplateFormatMethod("messageTemplate")]
    public void Error(Exception ex, string? messageTemplate, params object[] propertyValues)
    {
        messageTemplate ??= "An exception occurred: {ExceptionMessage}";
        logger.Error(ex, messageTemplate, propertyValues);
    }
}