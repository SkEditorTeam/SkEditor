using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Services.Localization;
using SkEditor.Services.Settings;
using SkEditor.Services.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Extensions;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;
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

#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        base.OnFrameworkInitializationCompleted();

        Logger serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SkEditor/v3.0/Logs/log-.txt"), rollingInterval: RollingInterval.Minute)
            .WriteTo.Sink(new LogsHandler())
            .CreateLogger();

        Log.Logger = serilogLogger;

        ServiceCollection collection = new();
        collection.AddSingleton<ILogger>(serilogLogger);

        string[] appArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

        ConfigureServices(collection, _binarySettings, appArgs);
        Services = collection.BuildServiceProvider();
        TranslationService.Current = Services.GetRequiredService<ITranslationService>();

        SettingsObserverService observer = Services.GetRequiredService<SettingsObserverService>();
        observer.Initialize();

        Stopwatch.Start();

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        if (Services.GetRequiredService<SplashScreenService>().ShouldShowSplash())
        {
            SplashScreen splashScreen = Services.GetRequiredService<SplashScreen>();
            _splashScreen = splashScreen;
            desktop.MainWindow = _splashScreen;
            _splashScreen.Show();
        }

        Dispatcher.UIThread.Post(() =>
        {
            IStartupService startupService = Services.GetRequiredService<IStartupService>();
            startupService.CompleteApplicationStartAsync(desktop, _splashScreen);
        }, DispatcherPriority.Background);
    }

    private static void ConfigureServices(IServiceCollection services, BinarySettings binarySettings, string[] args)
    {
        services.AddSingleton(args);
        services.AddSingleton<IClassicDesktopStyleApplicationLifetime>(_ =>
            (IClassicDesktopStyleApplicationLifetime)Current!.ApplicationLifetime!);

        services.AddCoreServices();
        services.AddWindowing();
        services.AddFileServices();

        services.AddViewModels();
        services.AddViews();

        services.AddConfigs(binarySettings);
        services.AddEditorFeatures();
        services.AddEditorCommands();
        services.AddTabs();
        services.AddSidebar();
        services.AddStatusBar();

        services.AddAnalyzer();
    }

    public static string GetInformationalVersion()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string? informationVersion =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        return informationVersion ?? throw new InvalidOperationException("Informational version is null");
    }
}