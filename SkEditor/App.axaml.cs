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
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Services;
using SkEditor.Services.Files;
using SkEditor.Services.Files.Features;
using SkEditor.Services.Localization;
using SkEditor.Services.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Publishing;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Startup;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Pages;
using SkEditor.ViewModels.Windows;
using SkEditor.Views.Pages;
using SkEditor.Views.Windows;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;
using MainWindow = SkEditor.Views.Windows.MainWindow;
using SplashScreen = SkEditor.Views.Windows.SplashScreen;

namespace SkEditor;

public class App : Application
{
    private readonly BinarySettings _binarySettings;
    private SplashScreen? _splashScreen;

    public App()
    {
        _binarySettings = new BinarySettings();
        _binarySettings.Load();
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

        ConfigureServices(collection, _binarySettings);
        Services = collection.BuildServiceProvider();
        TranslationService.Current = Services.GetRequiredService<ITranslationService>();

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

    private static void ConfigureServices(IServiceCollection services, BinarySettings binarySettings)
    {
        services.AddSingleton<MainWindowFactory>();
        services.AddSingleton<WindowRegistry>();
        services.AddSingleton<IRemoteConnectionService, RemoteConnectionService>();
        services.AddSingleton<IStartupService, StartupService>();
        services.AddSingleton<IMenuService, MenuService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<ILogService, LogService>();
        services.AddSingleton<IconProvider>();
        services.AddSingleton<CrashChecker>();
        services.AddSingleton<IThemeService, ThemeService>();
        services.AddSingleton<ITabService, TabService>();
        services.AddSingleton<IFileService, FileService>();
        services.AddSingleton<IFileTabFactory, FileTabFactory>();
        services.AddSingleton<IWelcomeService, WelcomeService>();
        services.AddSingleton<IConfigService, ConfigService>();
        services.AddSingleton<LegacyConfigMigrator>();
        services.AddSingleton<ITranslationService, TranslationService>();
        services.AddSingleton<IDiscordIntegrationService, DiscordIntegrationService>();
        services.AddSingleton<IClipboardService, ClipboardService>();
        services.AddSingleton<IFilePickerService, FilePickerService>();
        services.AddSingleton<IZoomService, ZoomService>();

        services.AddSingleton<AppSettings>();
        services.AddSingleton<IConfig>(sp => sp.GetRequiredService<AppSettings>());
        services.AddSingleton<ConnectionsConfig>();
        services.AddSingleton<IConfig>(sp => sp.GetRequiredService<ConnectionsConfig>());
        services.AddSingleton<ExperimentsConfig>();
        services.AddSingleton<IConfig>(sp => sp.GetRequiredService<ExperimentsConfig>());
        services.AddSingleton<SecretsConfig>();
        services.AddSingleton<IConfig>(sp => sp.GetRequiredService<SecretsConfig>());
        services.AddSingleton<StateConfig>();
        services.AddSingleton<IConfig>(sp => sp.GetRequiredService<StateConfig>());
        services.AddSingleton(binarySettings);
        services.AddSingleton<IConfig>(sp => sp.GetRequiredService<BinarySettings>());

        services.AddHttpClient<IPublishService, PublishService>();

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<RemoteConnectionViewModel>();
        services.AddTransient<RemoteSidebarViewModel>();
        services.AddTransient<FileTreeViewModel>();
        services.AddTransient<ThemePageViewModel>();
        services.AddTransient<WhatsNewPageViewModel>();
        services.AddTransient<WelcomePageViewModel>();
        services.AddTransient<PublishWindowViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<SplashScreen>();
        services.AddTransient<RemoteConnectionWindow>();
        services.AddTransient<WhatsNewPage>();
        services.AddTransient<PublishWindow>();

        services.AddSingleton<IFileFeatureService, FileFeatureService>();
        services.AddEditorFeatures();
    }
}