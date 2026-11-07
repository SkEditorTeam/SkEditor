using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using SkEditor.Configuration;
using SkEditor.Core.Logging;
using SkEditor.Windowing.UI;

namespace SkEditor.Windowing.Errors;

public class WindowErrorService(IConfigService configs, ICrashService crashes, ILogService logs)
{
    private int _crashHandlingStarted;

    public void ConfigureErrorHandling()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            if (Interlocked.Exchange(ref _crashHandlingStarted, 1) != 0)
                return;
            
            try
            {
                var exception = e.ExceptionObject as Exception
                                ?? new Exception($"Non-Exception object: {e.ExceptionObject}");

                logs.Fatal(exception, "Application crashed!");
                crashes.SaveCrashState(exception);
            }
            catch
            {
                // ignored
            }
        };

        Dispatcher.UIThread.UnhandledException += (__, e) =>
        {
            e.Handled = true;

            if (Interlocked.Exchange(ref _crashHandlingStarted, 1) != 0)
                return;

            _ = HandleCrashAsync(e.Exception);
        };
    }

    private async Task HandleCrashAsync(Exception ex)
    {
        try
        {
            logs.Fatal(ex, "Application crashed!");

            await configs.SaveAllConfigsImmediateAsync();

            crashes.SaveCrashState(ex);

            await Task.Delay(500);

            if (ShouldAttemptRestart())
            {
                string? processPath = Environment.ProcessPath;

                if (!string.IsNullOrEmpty(processPath))
                {
                    Process.Start(processPath, "--crash");
                }
            }
        }
        catch (Exception crashHandlingException)
        {
            try
            {
                logs.Error(crashHandlingException, "Crash handling failed!");
            }
            catch
            {
                // ignored
            }
        }
        finally
        {
            Environment.Exit(1);
        }
    }

    private static bool ShouldAttemptRestart()
    {
#if DEBUG
        return false;
#else
        
        bool alreadyCrashed = Environment.GetCommandLineArgs()
            .Any(arg => arg.Equals("--crash", StringComparison.OrdinalIgnoreCase));
        TimeSpan uptime = DateTime.Now - Process.GetCurrentProcess().StartTime;

        return !alreadyCrashed || uptime >= TimeSpan.FromSeconds(10);
#endif
    }

    public async Task CheckForErrors(IClassicDesktopStyleApplicationLifetime desktop, SplashScreen? splashScreen,
        Task t)
    {
        if (!t.IsFaulted || t.Exception == null)
        {
            return;
        }

        AggregateException flattenedException = t.Exception.Flatten();
        Exception innermost = flattenedException.InnerExceptions.FirstOrDefault() ?? flattenedException;

        logs.Error(t.Exception, $"Error creating SkEditor: {innermost.Message}\nStack Trace: {innermost.StackTrace}");

        await splashScreen.TryUpdateStatusAsync(
            $"Error: {t.Exception.InnerException?.Message ?? t.Exception.Message}");

        await Dispatcher.UIThread.InvokeAsync(() => desktop.Shutdown(),
            DispatcherPriority.Background,
            new CancellationTokenSource(3000).Token);
    }
}