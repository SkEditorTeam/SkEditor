using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FluentAvalonia.UI.Windowing;
using Serilog;
using SkEditor.API;
using SkEditor.Contracts.Services;
using SkEditor.Services.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Views.Windows;

namespace SkEditor.Services;

public class StartupService(
    MainWindowFactory mainWindowFactory,
    WindowInstanceService windowInstanceService,
    WindowStateService windowStateService,
    WindowErrorService windowErrorService,
    IconProvider iconProvider,
    IConfigService config,
    AppSettings appSettings,
    LegacyConfigMigrator configMigrator) : IStartupService
{
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

        SkEditorAPI.Core.SetStartupArguments(desktop.Args ?? []);

        await Dispatcher.UIThread.InvokeAsync(() => FAAppWindow.AlwaysFallback = appSettings.ForceNativeTitleBar);

        await configMigrator.MigrateConfigsAsync();
        await config.LoadAllConfigsAsync();
        await iconProvider.LoadIcons();

        MainWindow mainWindow = await Dispatcher.UIThread.InvokeAsync(() => mainWindowFactory.Create(splashScreen));

        NamedPipeServer.Start();

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            windowStateService.RecoverMainWindowState(mainWindow);
            desktop.MainWindow = mainWindow;
            mainWindow.ApplyTemplate();
        });
    }
}