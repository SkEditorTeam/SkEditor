using System.Threading.Tasks;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using HotAvalonia;
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