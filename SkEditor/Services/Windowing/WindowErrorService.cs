using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Serilog;
using SkEditor.Contracts.Services;
using SkEditor.Views.Windows;
// ReSharper disable RedundantUsingDirective
using System.Text;
using System.Diagnostics;

// ReSharper restore RedundantUsingDirective

namespace SkEditor.Services.Windowing;

public class WindowErrorService(IConfigService configs)
{
    public void ConfigureErrorHandling()
    {
        Dispatcher.UIThread.UnhandledException += async (_, e) =>
        {
            e.Handled = true;

            const string message = "Application crashed!";
            Log.Fatal(e.Exception, message);
            Console.Error.WriteLine(e);
            await Console.Error.WriteLineAsync(message);

            await configs.SaveAllConfigsImmediateAsync();

#if !DEBUG
            string fullException = e.Exception.ToString();
            string encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(fullException));
            await Task.Delay(500);

            string? processPath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(processPath))
            {
                processPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            Process.Start(processPath, "--crash " + encodedMessage);
#endif
            Environment.Exit(1);
        };
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

        Log.Error(t.Exception, "Error creating SkEditor: {ExceptionMessage}\nStack Trace: {StackTrace}",
            innermost.Message,
            innermost.StackTrace);

        await splashScreen.TryUpdateStatusAsync(
            $"Error: {t.Exception.InnerException?.Message ?? t.Exception.Message}");

        await Dispatcher.UIThread.InvokeAsync(() => desktop.Shutdown(),
            DispatcherPriority.Background,
            new CancellationTokenSource(3000).Token);
    }
}