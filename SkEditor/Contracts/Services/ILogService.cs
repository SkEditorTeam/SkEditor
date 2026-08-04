using System;
using R3;
using SkEditor.Models.Logging;

namespace SkEditor.Contracts.Services;

/// <summary>
///     A service used for all logging purposes.
/// </summary>
public interface ILogService
{
    string LogsFolderPath { get; }
    Observable<LogEntry> OnLogReceived { get; }
    
    /// <summary>
    ///     Logs a debug message using a message template.
    ///     Should be used for development and debugging purposes.
    /// </summary>
    /// <param name="messageTemplate">The message template with placeholders like {Property}</param>
    /// <param name="source">The source of the log entry</param>
    /// <param name="propertyValues">Values to fill the placeholders</param>
    void Debug(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues);

    /// <summary>
    ///     Logs an informational message using a message template.
    ///     Used for general information about the application's state or actions.
    /// </summary>
    /// <param name="messageTemplate">The message template with placeholders like {Property}</param>
    /// <param name="source">The source of the log entry</param>
    /// <param name="propertyValues">Values to fill the placeholders</param>
    void Info(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues);

    /// <summary>
    ///     Logs a warning message using a message template.
    ///     Used for potential issues that are not necessarily errors but may require attention.
    /// </summary>
    /// <param name="messageTemplate">The message template with placeholders like {Property}</param>
    /// <param name="source">The source of the log entry</param>
    /// <param name="propertyValues">Values to fill the placeholders</param>
    void Warning(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues);

    /// <summary>
    ///     Logs an error message using a message template.
    ///     Used for errors that occur during the app's execution.
    /// </summary>
    /// <param name="messageTemplate">The message template with placeholders like {Property}</param>
    /// <param name="source">The source of the log entry</param>
    /// <param name="propertyValues">Values to fill the placeholders</param>
    void Error(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues);

    /// <summary>
    ///     Logs a fatal message using a message template.
    ///     Used for critical errors that cause the application to crash or become unusable.
    /// </summary>
    /// <param name="messageTemplate">The message template with placeholders like {Property}</param>
    /// <param name="source">The source of the log entry</param>
    /// <param name="propertyValues">Values to fill the placeholders</param>
    void Fatal(string messageTemplate, LogSource source = LogSource.SkEditor, params object[] propertyValues);

    /// <summary>
    ///     Logs a fatal message from an exception with an optional custom message template.
    /// </summary>
    /// <param name="exception">The captured exception</param>
    /// <param name="messageTemplate">Optional message template with placeholders like {Property}</param>
    /// <param name="source">The source of the log entry</param>
    /// <param name="propertyValues">Values to fill the placeholders</param>
    void Fatal(Exception exception, string? messageTemplate = null, LogSource source = LogSource.SkEditor,
        params object[] propertyValues);

    /// <summary>
    ///     Logs an error from an exception with an optional custom message template.
    /// </summary>
    /// <param name="exception">The captured exception</param>
    /// <param name="messageTemplate">Optional message template with placeholders like {Property}</param>
    /// <param name="source">The source of the log entry</param>
    /// <param name="propertyValues">Values to fill the placeholders</param>
    void Error(Exception exception, string? messageTemplate = null, LogSource source = LogSource.SkEditor,
        params object[] propertyValues);
}