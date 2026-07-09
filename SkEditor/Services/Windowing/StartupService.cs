using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using FluentAvalonia.UI.Windowing;
using Serilog;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Analyzer;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Files;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class StartupService(
    MainWindowFactory mainWindowFactory,
    WindowInstanceService windowInstanceService,
    WindowStateService windowStateService,
    WindowErrorService windowErrorService,
    NamedPipeServer namedPipeServer,
    IConfigService config,
    AppSettings appSettings,
    LegacyConfigMigrator configMigrator,
    IAnalyzer analyzer,
    IFileService files,
    string[] args) : IStartupService
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

        MainWindow mainWindow = await Dispatcher.UIThread.InvokeAsync(() => mainWindowFactory.Create(splashScreen));

        namedPipeServer.Start();

        //_ = analyzer.Initialize();

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            windowStateService.RecoverMainWindowState(mainWindow);
            desktop.MainWindow = mainWindow;
            mainWindow.ApplyTemplate();

            await CheckArgs();
        });
    }

    private async Task CheckArgs()
    {
        foreach (string path in args)
        {
            if (File.Exists(path))
            {
                await files.OpenFileAsync(new LocalFile(path));
            }
        }
    }
}