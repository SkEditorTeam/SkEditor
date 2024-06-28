using System;

namespace SkEditor.API;

/// <summary>
/// Interface for logging.
/// </summary>
public interface ILogs
{
    
    /// <summary>
    /// Sends a debug message to the log. If SkEditor's developer mode is
    /// enabled, this message will be displayed in the bottom bar.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Debug(string message);
    
    /// <summary>
    /// Sends an info message to the log.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Info(string message);
    
    /// <summary>
    /// Sends a warning message to the log.
    /// </summary>
    /// <param name="message">The warning message to log.</param>
    public void Warning(string message);

    /// <summary>
    /// Sends an error message to the log. Optionally informs the user.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="informUser">Whether to inform the user about the error. Default is false.</param>
    public void Error(string message, bool informUser = false);

    /// <summary>
    /// Sends a fatal error message to the log. This type of error is critical and may cause the program to terminate.
    /// </summary>
    /// <param name="message">The fatal error message to log.</param>
    public void Fatal(string message);
    
    /// <summary>
    /// Sends a fatal exception to the log.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    public void Fatal(Exception exception);
    
}