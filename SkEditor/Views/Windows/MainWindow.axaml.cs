using System.Threading.Tasks;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using HotAvalonia;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class MainWindow : FAAppWindow
{
    private readonly AppSettings _appSettings;
    private readonly IDiscordIntegrationService _discordIntegration;
    private readonly SplashScreenService _splashScreen;
    private readonly IThemeService _themes;
    private readonly ITranslationService _translation;
    private readonly IWindowCloseService _windowClose;

    public MainWindow()
    {
        InitializeComponent();
        _appSettings = null!;
        _translation = null!;
        _themes = null!;
        _discordIntegration = null!;
        _splashScreen = null!;
        _windowClose = null!;
    }

    public MainWindow(
        MainWindowViewModel viewModel,
        MainMenuViewModel menuViewModel,
        AppSettings appSettings,
        IThemeService themes,
        ITranslationService translation,
        DragDropService dragDropService,
        ShortcutsService shortcutsService,
        IDiscordIntegrationService discordIntegration,
        IWindowCloseService windowClose,
        SplashScreenService splashScreen)
        : this()
    {
        DataContext = viewModel;
        _appSettings = appSettings;
        _translation = translation;
        _themes = themes;
        _discordIntegration = discordIntegration;
        _splashScreen = splashScreen;
        _windowClose = windowClose;

        menuViewModel.RegisterHotKeys(this);

        _themes.LoadThemes();
        Style();

        LayoutTransform.LayoutTransform = new ScaleTransform(appSettings.CustomUiScale, appSettings.CustomUiScale);

        AttachEvents(dragDropService, shortcutsService);
    }

    public new SplashScreen? SplashScreen { get; set; }

    private void AttachEvents(DragDropService dragDropService, ShortcutsService shortcutsService)
    {
        Loaded += async (_, _) => await OnLoaded();
        Closing += async (_, e) =>
        {
            if (_windowClose.IsClosing)
            {
                return;
            }

            e.Cancel = true;
            bool close = await _windowClose.OnWindowClosing(Height, Width, Position.X, Position.Y, WindowState);
            if (close)
            {
                Close();
            }
        };
        dragDropService.AttachDragDropEvents(this);
        shortcutsService.AttachKeyEvents(this);
    }

    private void Style()
    {
        const double titleBarHeight = 50;

        TitleBar.ExtendsContentIntoTitleBar = true;

        TitleBar.Height = titleBarHeight;
        ExtendClientAreaTitleBarHeightHint = titleBarHeight;
    }

    public async Task OnLoaded()
    {
        await _translation.ChangeLanguageAsync(_appSettings.Language, true);
        await SplashScreen.TryUpdateStatusAsync("Loading themes and addons...");

        await _themes.SetThemeAsync(_themes.CurrentTheme);

        App.Stopwatch.Stop();
        _splashScreen.UpdateStartupTime((int)App.Stopwatch.ElapsedMilliseconds);

        await SplashScreen.TryUpdateStatusAsync("Initializing main window...");

        _discordIntegration.SetUp();

        Dispatcher.Post(() =>
        {
            IsVisible = true;
            Activate();
            Focus();

            App.Services.GetRequiredService<ILogService>().Debug(
                "This is a debug message, which is only visible in debug builds. If you see this in release, something is wrong.");
            App.Services.GetRequiredService<ILogService>()
                .Info($"Main window loaded in {App.Stopwatch.ElapsedMilliseconds}ms");
            App.Services.GetRequiredService<ILogService>().Info($"Current theme: {_themes.CurrentTheme}");
            App.Services.GetRequiredService<ILogService>()
                .Info("Having any issues? Join our Discord: https://discord.gg/SkEditor");
            App.Services.GetRequiredService<ILogService>().Info("Have a great day! :)");
            App.Services.GetRequiredService<ILogService>()
                .Warning("Ooohh, that's a warning, so we're completely ignoring it");
            App.Services.GetRequiredService<ILogService>()
                .Error("This is an error message, but it's not a real error, so don't worry about it.");
            App.Services.GetRequiredService<ILogService>().Fatal(
                "Scary, right? This is a fatal error message, but it's not a real fatal error, so don't worry about it.");

            SplashScreen?.Close();
        });
    }

    [AvaloniaHotReload]
    public void Reload()
    {
        _ = _translation.ChangeLanguageAsync(_appSettings.Language);
        _ = _themes.SetThemeAsync(_themes.CurrentTheme);
    }
}