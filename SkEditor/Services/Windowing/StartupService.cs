using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FluentAvalonia.UI.Windowing;
using Serilog;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class StartupService(
    MainWindowFactory mainWindowFactory,
    WindowInstanceService windowInstanceService,
    WindowStateService windowStateService,
    WindowErrorService windowErrorService,
    NamedPipeServer namedPipeServer,
    IconProvider iconProvider,
    IConfigService config,
    AppSettings appSettings,
    LegacyConfigMigrator configMigrator) : IStartupService
{
    public string[]? StartupArguments { get; set; }

    public async Task CompleteApplicationStartAsync(
        IClassicDesktopStyleApplicationLifetime desktop,
        SplashScreen? splashScreen)
    {
        try
        {
            _ = Task.Run(() => RunStartupAsync(desktop, splashScreen))
                .ContinueWith(
                    async t => await windowErrorService.CheckForErrors(desktop, splashScreen, t),
                    TaskScheduler.Default);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating SkEditor");

            if (splashScreen is not null)
            {
                await splashScreen.TryUpdateStatusAsync($"Error: {ex.Message}");
            }

            using CancellationTokenSource cts = new(TimeSpan.FromSeconds(3));
            await Dispatcher.UIThread.InvokeAsync(
                () => desktop.Shutdown(),
                DispatcherPriority.Background,
                cts.Token);
        }
    }

    private async Task RunStartupAsync(
        IClassicDesktopStyleApplicationLifetime desktop,
        SplashScreen? splashScreen)
    {
        windowErrorService.ConfigureErrorHandling();

        bool continueStartup =
            await Dispatcher.UIThread.InvokeAsync(() => windowInstanceService.HandleSingleInstance(desktop));

        if (!continueStartup)
        {
            return;
        }

        StartupArguments = desktop.Args ?? [];

        await Dispatcher.UIThread.InvokeAsync(() => FAAppWindow.AlwaysFallback = appSettings.ForceNativeTitleBar);

        await configMigrator.MigrateConfigsAsync();
        await config.LoadAllConfigsAsync();
        await iconProvider.LoadIcons();

        MainWindow mainWindow = await Dispatcher.UIThread.InvokeAsync(() => mainWindowFactory.Create(splashScreen));

        namedPipeServer.Start();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            windowStateService.RecoverMainWindowState(mainWindow);
            desktop.MainWindow = mainWindow;
            mainWindow.ApplyTemplate();
        });
    }
}