using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using SkEditor.API;
using SkEditor.Contracts.Services;
using SkEditor.Services;
using SkEditor.Services.Files;
using SkEditor.Services.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Startup;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Pages;
using SkEditor.ViewModels.Windows;
using SkEditor.Views.Windows;
using SkEditor.Views.Windows.Settings;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;
using MainWindow = SkEditor.Views.Windows.MainWindow;
using SplashScreen = SkEditor.Views.Windows.SplashScreen;

namespace SkEditor;

public class App : Application
{
    private SplashScreen? _splashScreen;

    public App()
    {
        BinarySettings binarySettings = new();
        SkEditorAPI.Configs.AddConfig(binarySettings);
        binarySettings.Load();
    }

    public static IServiceProvider Services { get; private set; } = null!;

    public static Stopwatch Stopwatch { get; } = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        Logger serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SkEditor/Logs/log-.txt"), rollingInterval: RollingInterval.Minute)
            .WriteTo.Sink(new LogsHandler())
            .CreateLogger();

        Log.Logger = serilogLogger;

        ServiceCollection collection = new();
        collection.AddSingleton<ILogger>(serilogLogger);

        ConfigureServices(collection);
        Services = collection.BuildServiceProvider();

        Stopwatch.Start();

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        if (SplashScreenHandler.ShouldShowSplash())
        {
            SplashScreen splashScreen = Services.GetRequiredService<SplashScreen>();
            _splashScreen = splashScreen;
            desktop.MainWindow = _splashScreen;
            _splashScreen.Show();
        }

        Dispatcher.UIThread.Post(() =>
        {
            IStartupService startupService = Services.GetRequiredService<IStartupService>();
            startupService.CompleteApplicationStart(desktop, _splashScreen);
        }, DispatcherPriority.Background);
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MainWindowFactory>();
        services.AddSingleton<SkEditorSelfAddon>();
        services.AddSingleton<WindowRegistry>();
        services.AddSingleton<IRemoteConnectionService, RemoteConnectionService>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<IMenuService, MenuService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<IconProvider>();
        services.AddSingleton<CrashChecker>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<IFileFactory, FileFactory>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<RemoteConnectionViewModel>();
        services.AddTransient<RemoteSidebarViewModel>();
        services.AddTransient<FileTreeViewModel>();
        services.AddTransient<ThemePageViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<SplashScreen>();
        services.AddTransient<RemoteConnectionWindow>();
        services.AddTransient<SettingsWindow>();
    }
}