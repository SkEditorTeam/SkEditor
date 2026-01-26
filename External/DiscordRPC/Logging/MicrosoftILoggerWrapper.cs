using Microsoft.Extensions.Logging;
using System;

namespace DiscordRPC.Logging
{
    /// <summary>
    /// An implementation of <see cref="ILogger"/> that wraps a Microsoft.Extensions.Logging.ILogger instance.
    /// Forwards log messages to the provided Microsoft logger.
    /// </summary>
    public class MicrosoftILoggerWrapper(Microsoft.Extensions.Logging.ILogger logger) : ILogger
    {
        /// <inheritdoc/>
        public LogLevel Level { get; set; }

        /// <summary>
        /// Formats the log message using string.Format with the provided arguments.
        /// </summary>
        /// <param name="message">The message template.</param>
        /// <param name="args">Arguments to format into the message.</param>
        /// <returns>The formatted message string.</returns>
        private string FormatMessage(string message, params object[] args)
        {
            return string.Format(message, args);
        }

        /// <inheritdoc/>
        public void Trace(string message, params object[] args)
        {
#if DEBUG
            logger.LogTrace(FormatMessage(message, args));
#endif
        }

        /// <inheritdoc/>
        public void Info(string message, params object[] args)
        {
            logger.LogInformation(FormatMessage(message, args));
        }

        /// <inheritdoc/>
        public void Warning(string message, params object[] args)
        {
            logger.LogWarning(FormatMessage(message, args));
        }

        /// <inheritdoc/>
        public void Error(string message, params object[] args)
        {
            logger.LogError(FormatMessage(message, args));
        }

        /// <summary>
        /// Logs an error message with an associated exception.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="message">The error message template.</param>
        /// <param name="args">Arguments to format into the message.</param>
        public void Error(Exception ex, string message, params object[] args)
        {
            logger.LogError(ex, FormatMessage(message, args));
        }
    }
}