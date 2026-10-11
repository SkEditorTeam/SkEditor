using System;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Services.Localization;
using SkEditor.Services.Settings;
using SkEditor.Services.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Extensions;
using SkEditor.ViewModels;
using BinarySettings = SkEditor.Utilities.Settings.Configs.BinarySettings;
using SplashScreen = SkEditor.Views.Windows.SplashScreen;

namespace SkEditor;

public class App : Application
{
    private readonly BinarySettings _binarySettings;
    private SplashScreen? _splashScreen;

    public static IServiceProvider Services { get; private set; } = null!;

    public static Stopwatch Stopwatch { get; } = new();

    public App()
    {
        _binarySettings = new BinarySettings();
        _binarySettings.Load();
    }

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

        SetUpLinux();

        MigrateLogDirectory();

        Logger serilogLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(e =>
                    e.Properties.TryGetValue("Source", out LogEventPropertyValue? val) &&
                    val.ToString().Contains("Analyzer"))
                .WriteTo.Async(a => a.File(
                    Path.Combine(AppPaths.Logs, "log-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7)))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(e =>
                    e.Properties.TryGetValue("Source", out LogEventPropertyValue? val) &&
                    val.ToString().Contains("Analyzer"))
                .WriteTo.Async(a => a.File(
                    Path.Combine(AppPaths.Logs, "Analyzer", "analyzer-.txt"),
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 3)))
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();

        Log.Logger = serilogLogger;

        ServiceCollection collection = new();
        collection.AddSingleton<ILogger>(serilogLogger);

        string[] appArgs = Environment.GetCommandLineArgs().Skip(1).ToArray();

        ConfigureServices(collection, _binarySettings, appArgs);
        Services = collection.BuildServiceProvider();
        TranslationService.Current = Services.GetRequiredService<ITranslationService>();
        DataContext = Services.GetRequiredService<AppViewModel>();

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

        desktop.Exit += (_, _) => { (Services as IDisposable)?.Dispose(); };

        Dispatcher.UIThread.Post(() =>
        {
            IStartupService startupService = Services.GetRequiredService<IStartupService>();
            startupService.CompleteApplicationStartAsync(desktop, _splashScreen);
        }, DispatcherPriority.Background);
    }

    public static string GetInformationalVersion()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();
        string? informationVersion =
            assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        return informationVersion ?? throw new InvalidOperationException("Informational version is null");
    }

    // TEMP: Remove migration from Roaming when leaving Alpha
    private static void MigrateLogDirectory()
    {
        try
        {
            string legacyRoot = Path.Combine(AppPaths.RoamingData, "Logs");

            if (Directory.Exists(AppPaths.Logs) || !Directory.Exists(legacyRoot))
            {
                return;
            }

            Directory.Move(legacyRoot, AppPaths.Logs);
        }
        catch
        {
            // ignored
        }
    }

    private void SetUpLinux()
    {
        if (!OperatingSystem.IsLinux())
        {
            return;
        }

        Style expanderStyle = new(x => x.OfType<Expander>());
        expanderStyle.Setters.Add(new Setter(FAExpanderExt.ExpanderAnimationTypeProperty, null));
        Styles.Add(expanderStyle);
    }

    private static void ConfigureServices(IServiceCollection services, BinarySettings binarySettings, string[] args)
    {
        services.AddSingleton<AppViewModel>();
        services.AddSingleton(args);
        services.AddSingleton<IClassicDesktopStyleApplicationLifetime>(_ =>
            (IClassicDesktopStyleApplicationLifetime)Current!.ApplicationLifetime!);

        services.AddSingleton<IFileSystem, FileSystem>();

        services.AddCoreServices();
        services.AddWindowing();
        services.AddFileServices();

        services.AddViewModels();
        services.AddViews();

        services.AddConfigs(binarySettings);
        services.AddEditorFeatures();
        services.AddEditorCommands();
        services.AddEditorCompletions();
        services.AddTabs();
        services.AddSidebar();
        services.AddStatusBar();

        services.AddAnalyzer();
    }
}