using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.API;
using SkEditor.Contracts.Services;
using SkEditor.Services;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Startup;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Windows;
using SkEditor.Views.Windows;
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

        var collection = new ServiceCollection();
        ConfigureServices(collection);
        Services = collection.BuildServiceProvider();

        Stopwatch.Start();

        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        if (SplashScreenHandler.ShouldShowSplash())
        {
            var splashScreen = Services.GetRequiredService<SplashScreen>();
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

        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<MainMenuViewModel>();
        services.AddTransient<RemoteConnectionViewModel>();
        services.AddTransient<FileTreeViewModel>();

        services.AddTransient<MainWindow>();
        services.AddTransient<SplashScreen>();
        services.AddTransient<RemoteConnectionWindow>();
    }
}