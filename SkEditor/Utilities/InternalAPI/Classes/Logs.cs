using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Threading;
using Serilog;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.API;

/// <summary>
///     Provides logging capabilities for SkEditor and its addons
/// </summary>
public class Logs : ILogs
{
    /// <summary>
    ///     Logs a debug message
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Debug(string message)
    {
        string source = GetSourceName();
        Log.Debug("[{Source}] {Message}", source, message);
    }

    /// <summary>
    ///     Logs an informational message
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Info(string message)
    {
        string source = GetSourceName();
        Log.Information("[{Source}] {Message}", source, message);
    }

    /// <summary>
    ///     Logs a warning message
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Warning(string message)
    {
        string source = GetSourceName();
        Log.Warning("[{Source}] {Message}", source, message);
    }

    /// <summary>
    ///     Logs an error message
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="informUser">Whether to show an error dialog to the user</param>
    public void Error(string message, bool informUser = false)
    {
        string source = GetSourceName();
        Log.Error("[{Source}] {Message}", source, message);

        if (informUser)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await SkEditorAPI.Windows.ShowError(message));
        }
    }

    /// <summary>
    ///     Logs a fatal message
    /// </summary>
    /// <param name="message">The message to log</param>
    public void Fatal(string message)
    {
        string source = GetSourceName();
        Log.Fatal("[{Source}] {Message}", source, message);
    }

    /// <summary>
    ///     Logs a fatal exception
    /// </summary>
    /// <param name="exception">The exception to log</param>
    public void Fatal(Exception exception)
    {
        string source = GetSourceName();
        Log.Fatal(exception, "[{Source}] {Message}", source, exception.Message);
        Log.Fatal("[{Source}] {StackTrace}", source, exception.StackTrace);
    }

    /// <summary>
    ///     Logs an error message from an addon
    /// </summary>
    /// <param name="message">The message to log</param>
    /// <param name="informUser">Whether to show an error dialog to the user</param>
    /// <param name="addon">The addon that caused the error. If null, it will be detected automatically</param>
    public void AddonError(string message, bool informUser = false, IAddon? addon = null)
    {
        addon ??= DetectAddonFromStackTrace();

        Log.Error("[{Source}] {Message}", addon?.Name ?? "Addon not Found", message);

        if (informUser)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await SkEditorAPI.Windows.ShowError(message));
        }
    }

    /// <summary>
    ///     Returns the name of the source of the log message (addon name or "SkEditor")
    /// </summary>
    private static string GetSourceName()
    {
#if !AOT
        MethodBase? methodInfo = new StackTrace().GetFrame(2)?.GetMethod();
        string? callerNamespace = methodInfo?.ReflectedType?.Namespace;
        IAddon? addon = AddonLoader.GetAddonByNamespace(callerNamespace);
        return addon?.Name ?? "SkEditor";
#else
        return "SkEditor";
#endif
    }

    /// <summary>
    ///     Detects the addon that caused the error from the stack trace
    /// </summary>
    private IAddon? DetectAddonFromStackTrace()
    {
        StackFrame[] frames = new StackTrace().GetFrames();
        string? addonNamespace = null;

        foreach (StackFrame frame in frames)
        {
            MethodBase? method = frame.GetMethod();
            if (method?.ReflectedType?.Namespace?.StartsWith("SkEditor") != false)
            {
                continue;
            }

            addonNamespace = method.ReflectedType?.Namespace;
            break;
        }

        return AddonLoader.GetAddonByNamespace(addonNamespace);
    }
}