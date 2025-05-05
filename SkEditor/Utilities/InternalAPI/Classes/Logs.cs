using System;
using System.Diagnostics;
using System.Reflection;
using Avalonia.Threading;
using Serilog;
using SkEditor.Utilities.InternalAPI;

namespace SkEditor.API;

public class Logs : ILogs
{
    public void Debug(string message)
    {
        Log.Debug(FormatMessage(message));
    }

    public void Info(string message)
    {
        Log.Information(FormatMessage(message));
    }

    public void Warning(string message)
    {
        Log.Warning(FormatMessage(message));
    }

    public void Error(string message, bool informUser = false)
    {
        Log.Error(FormatMessage(message));
        if (informUser)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await SkEditorAPI.Windows.ShowError(message));
        }
    }

    public void Fatal(string message)
    {
        Log.Fatal(FormatMessage(message));
    }

    public void Fatal(Exception exception)
    {
        Log.Fatal(exception, FormatMessage(exception.Message));
        Log.Fatal(exception.StackTrace);
    }

    public void AddonError(string message, bool informUser = false, IAddon? addon = null)
    {
        if (addon == null)
        {
            StackFrame[] frames = new StackTrace().GetFrames();
            string? addonNamespace = null;
            foreach (StackFrame frame in frames)
            {
                MethodBase? method = frame.GetMethod();
                if (method.ReflectedType?.Namespace?.StartsWith("SkEditor") != false)
                {
                    continue;
                }

                addonNamespace = method.ReflectedType?.Namespace;
                break;
            }

            addon = AddonLoader.GetAddonByNamespace(addonNamespace);
        }

        Log.Error(FormatMessage($"[{addon?.Name ?? "Addon not Found"}] {message}"));
        if (informUser)
        {
            Dispatcher.UIThread.InvokeAsync(async () => await SkEditorAPI.Windows.ShowError(message));
        }
    }

    private static string FormatMessage(string message)
    {
#if !AOT
        MethodBase? methodInfo = new StackTrace().GetFrame(2)?.GetMethod();
        string? callerNamespace = methodInfo.ReflectedType?.Namespace;
        IAddon? addon = AddonLoader.GetAddonByNamespace(callerNamespace);
        return $"[{addon?.Name ?? "SkEditor"}] {message}";
#else
        return $"[SkEditor] {message}";
#endif
    }
}