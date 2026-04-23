using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using FluentAvalonia.UI.Windowing;
using Serilog;
using SkEditor.API;
using SkEditor.Contracts.Services;
using SkEditor.Services.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Views.Windows;
// ReSharper disable once RedundantUsingDirective
using System.Diagnostics;

namespace SkEditor.Services;

public class StartupService(
    MainWindowFactory mainWindowFactory,
    IconProvider iconProvider,
    IConfigService config,
    AppSettings appSettings,
    StateConfig stateConfig,
    LegacyConfigMigrator configMigrator) : IStartupService
{
    public async Task CompleteApplicationStartAsync(IClassicDesktopStyleApplicationLifetime desktop,
        SplashScreen? splashScreen)
    {
        try
        {
            await splashScreen.TryUpdateStatusAsync("Initializing...");

            _ = Task.Run(async () =>
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                await splashScreen.TryUpdateStatusAsync("Configuring error handling...");
                ConfigureErrorHandling();

                await splashScreen.TryUpdateStatusAsync("Checking for other instances...");

                bool continueStartup = await Dispatcher.UIThread.InvokeAsync(() => HandleSingleInstance(desktop));
                if (!continueStartup)
                {
                    return;
                }

                await splashScreen.TryUpdateStatusAsync("Loading configuration...");
                SkEditorAPI.Core.SetStartupArguments(desktop.Args ?? []);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    FAAppWindow.AlwaysFallback = appSettings.ForceNativeTitleBar;
                });

                await configMigrator.MigrateConfigsAsync();
                await config.LoadAllConfigsAsync();
                await iconProvider.LoadIcons();

                await splashScreen.TryUpdateStatusAsync("Creating main window...");

                MainWindow mainWindow =
                    await Dispatcher.UIThread.InvokeAsync(() => mainWindowFactory.Create(splashScreen));

                await splashScreen.TryUpdateStatusAsync("Starting named pipe server...");

                NamedPipeServer.Start();

                await splashScreen.TryUpdateStatusAsync("Initializing main window...");

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    RecoverMainWindowState(mainWindow);

                    desktop.MainWindow = mainWindow;
                    mainWindow.ApplyTemplate();
                });
            }).ContinueWith(async t =>
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
            }, TaskScheduler.Default);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating SkEditor");
            await splashScreen.TryUpdateStatusAsync($"Error: {ex.Message}");

            await Dispatcher.UIThread.InvokeAsync(() => desktop.Shutdown(),
                DispatcherPriority.Background,
                new CancellationTokenSource(3000).Token);
        }
    }

    private void RecoverMainWindowState(MainWindow mainWindow)
    {
        if (stateConfig is { MainWindowWidth: > 0, MainWindowHeight: > 0 })
        {
            mainWindow.Width = stateConfig.MainWindowWidth;
            mainWindow.Height = stateConfig.MainWindowHeight;
        }

        PixelPoint position = new(stateConfig.MainWindowX, stateConfig.MainWindowY);
        Screens screens = mainWindow.Screens;
        PixelPoint lookupPosition = new(
            (int)(stateConfig.MainWindowX + (stateConfig.MainWindowWidth / 2)),
            (int)(stateConfig.MainWindowY + (stateConfig.MainWindowHeight / 2))
        );

        Screen? screen = screens.ScreenFromPoint(lookupPosition)
                         ?? screens.ScreenFromPoint(new PixelPoint(stateConfig.MainWindowX,
                             stateConfig.MainWindowY));

        if (screen != null)
        {
            mainWindow.Position = stateConfig.MainWindowMaximized
                ? screen.Bounds.TopLeft
                : position;
        }
        else
        {
            mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        if (stateConfig.MainWindowMaximized)
        {
            mainWindow.WindowState = WindowState.Maximized;
        }
    }

    private static void ConfigureErrorHandling()
    {
        Dispatcher.UIThread.UnhandledException += async (_, e) =>
        {
            e.Handled = true;

            const string message = "Application crashed!";
            Log.Fatal(e.Exception, message);
            Console.Error.WriteLine(e);
            await Console.Error.WriteLineAsync(message);

            await SkEditorAPI.Core.SaveData();

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

    private static async Task<bool> HandleSingleInstance(IClassicDesktopStyleApplicationLifetime desktop)
    {
        Mutex mutex = new(true, $@"Local\{NamedPipeServer.PipeName}Mutex");

        bool isFirstInstance;
        try
        {
            isFirstInstance = mutex.WaitOne(TimeSpan.Zero, true);
        }
        catch (AbandonedMutexException ex)
        {
            Log.Debug(ex, "Abandoned mutex");
            isFirstInstance = true;
        }

        if (isFirstInstance)
        {
            desktop.Exit += (_, _) =>
            {
                mutex.ReleaseMutex();
                mutex.Dispose();
            };

            return true;
        }

        await SendArgsToExistingInstance();
        desktop.Shutdown();
        return false;
    }

    private static async Task SendArgsToExistingInstance()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            await using NamedPipeClientStream namedPipeClientStream = new(NamedPipeServer.PipeName);
            await namedPipeClientStream.ConnectAsync();
            if (args is { Length: > 1 })
            {
                byte[] buffer = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, args.Skip(1)));
                namedPipeClientStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                namedPipeClientStream.WriteByte(0);
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Error connecting to named pipe");
        }
    }
}