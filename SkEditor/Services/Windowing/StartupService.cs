using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using SkEditor.Analyzing;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Matcher;
using SkEditor.Models.Files;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class StartupService(
    MainWindowFactory mainWindowFactory,
    WindowInstanceService windowInstanceService,
    WindowStateService windowStateService,
    WindowErrorService windowErrorService,
    ISessionRestoreService sessionRestore,
    NamedPipeServer namedPipeServer,
    IConfigService config,
    LegacyConfigMigrator configMigrator,
    IAnalyzer analyzer,
    ISkriptSyntaxService syntaxService,
    IFileService files,
    ILogService logs,
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
            logs.Error(ex, "Error creating SkEditor");

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

        await configMigrator.MigrateConfigsAsync();
        await config.LoadAllConfigsAsync();

        MainWindow mainWindow = await Dispatcher.UIThread.InvokeAsync(() => mainWindowFactory.Create(splashScreen));

        namedPipeServer.Start();

        analyzer.ObserveOptionChange();
        _ = Task.Run(syntaxService.Load);

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            windowStateService.RecoverMainWindowState(mainWindow);
            await sessionRestore.RecoverSessionState();
            
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