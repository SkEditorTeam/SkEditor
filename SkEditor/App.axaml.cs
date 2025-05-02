using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Views;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkEditor;

public class App : Application
{
    private SplashScreen? _splashScreen;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) return;
        
        if (!desktop.Args.Contains("--hideSplashScreen"))
        {
            _splashScreen = new SplashScreen();
            desktop.MainWindow = _splashScreen;
            _splashScreen.Show();
        }

        Dispatcher.UIThread.Post(() => CompleteApplicationStart(desktop), DispatcherPriority.Background);
    }

    private void CompleteApplicationStart(IClassicDesktopStyleApplicationLifetime desktop)
    {
        try
        {
            _splashScreen?.UpdateStatus("Initializing...");

            Task.Run(async () =>
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                await Dispatcher.UIThread.InvokeAsync(() => _splashScreen?.UpdateStatus("Setting up logging..."));
                SetupLogging();

                await Dispatcher.UIThread.InvokeAsync(() =>
                    _splashScreen?.UpdateStatus("Configuring error handling..."));
                ConfigureErrorHandling();

                await Dispatcher.UIThread.InvokeAsync(() =>
                    _splashScreen?.UpdateStatus("Checking for other instances..."));

                bool continueStartup = await Dispatcher.UIThread.InvokeAsync(() => HandleSingleInstance(desktop));
                if (!continueStartup) return;

                await Dispatcher.UIThread.InvokeAsync(() => _splashScreen?.UpdateStatus("Loading configuration..."));
                SkEditorAPI.Core.SetStartupArguments(desktop.Args ?? []);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    FluentAvalonia.UI.Windowing.AppWindow.AlwaysFallback =
                        SkEditorAPI.Core.GetAppConfig().ForceNativeTitleBar;
                });

                await Dispatcher.UIThread.InvokeAsync(() => _splashScreen?.UpdateStatus("Creating main window..."));

                var mainWindow =
                    await Dispatcher.UIThread.InvokeAsync(() => new MainWindow(_splashScreen) { IsVisible = false });

                await Dispatcher.UIThread.InvokeAsync(() =>
                    _splashScreen?.UpdateStatus("Starting named pipe server..."));
                NamedPipeServer.Start();

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    desktop.MainWindow = mainWindow;
                    mainWindow.ApplyTemplate();
                    #if DEBUG
                    mainWindow.AttachDevTools();
                    #endif
                });
            }).ContinueWith(t =>
            {
                if (!t.IsFaulted || t.Exception == null) return;

                Log.Error(t.Exception, "Error creating SkEditor");
                Dispatcher.UIThread.Post(() =>
                {
                    _splashScreen?.UpdateStatus(
                        $"Error: {t.Exception.InnerException?.Message ?? t.Exception.Message}");

                    Dispatcher.UIThread.InvokeAsync(() => desktop.Shutdown(),
                        DispatcherPriority.Background,
                        new CancellationTokenSource(3000).Token);
                });
            }, TaskScheduler.Default);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating SkEditor");
            _splashScreen?.UpdateStatus($"Error: {ex.Message}");

            Dispatcher.UIThread.InvokeAsync(() => desktop.Shutdown(),
                DispatcherPriority.Background,
                new CancellationTokenSource(3000).Token);
        }
    }

    private static void SetupLogging()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SkEditor/Logs/log-.txt"), rollingInterval: RollingInterval.Minute)
            .WriteTo.Sink(new LogsHandler())
            .CreateLogger();
    }

    private static void ConfigureErrorHandling()
    {
        Dispatcher.UIThread.UnhandledException += async (_, e) =>
        {
            e.Handled = true;
            string? source = e.Exception.Source;
            if (AddonLoader.DllNames.Contains(source + ".dll"))
            {
                Log.Error(e.Exception, "An error occured in an addon: {Source}", source);
                await SkEditorAPI.Windows.ShowMessage("Error",
                    $"An error occured in an addon: {source}\n\n{e.Exception.Message}");
                return;
            }

            const string message = "Application crashed!";
            Log.Fatal(e.Exception, message);
            Console.Error.WriteLine(e);
            await Console.Error.WriteLineAsync(message);

            await SkEditorAPI.Core.SaveData();
            await AddonLoader.SaveMeta();

            var fullException = e.Exception.ToString();
            var encodedMessage = Convert.ToBase64String(Encoding.UTF8.GetBytes(fullException));
            await Task.Delay(500);
            Process.Start(Environment.ProcessPath, "--crash " + encodedMessage);
            Environment.Exit(1);
        };
    }

    private static bool HandleSingleInstance(IClassicDesktopStyleApplicationLifetime desktop)
    {
        Mutex mutex = new(true, "{217619cc-ff9d-438b-8a0a-348df94de61b}");

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

        SendArgsToExistingInstance();
        desktop.Shutdown();
        return false;
    }

    private static async void SendArgsToExistingInstance()
    {
        try
        {
            var args = Environment.GetCommandLineArgs();
            await using NamedPipeClientStream namedPipeClientStream = new("SkEditor");
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