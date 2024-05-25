using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Avalonia.Threading;

namespace SkEditor.API;

public class Logs : ILogs
{
    
    private string FormatMessage(string message)
    {
        var methodInfo = new StackTrace().GetFrame(2)?.GetMethod();
        var callerNamespace = methodInfo.ReflectedType?.Namespace;
        var addon = AddonLoader.GetAddonByNamespace(callerNamespace);
        return $"[{addon?.Name ?? "SkEditor"}] {message}";
    }
    
    public void Debug(string message)
    {
        Serilog.Log.Debug(FormatMessage(message));
    }
    
    public void Info(string message)
    {
        Serilog.Log.Information(FormatMessage(message));
    }
    
    public void Warning(string message)
    {
        Serilog.Log.Warning(FormatMessage(message));
    }
    
    public void Error(string message, bool informUser = false)
    {
        Serilog.Log.Error(FormatMessage(message));
        if (informUser)
            Dispatcher.UIThread.InvokeAsync(async () => await SkEditorAPI.Windows.ShowError(message));
    }
    
    public void Fatal(string message)
    {
        Serilog.Log.Fatal(FormatMessage(message));
    }

    public void Fatal(Exception exception)
    {
        Serilog.Log.Fatal(exception, FormatMessage(exception.Message));
    }
}