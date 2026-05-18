using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class MainWindow : FAAppWindow
{
    private readonly AppSettings _appSettings;
    private readonly IConfigService _configs;
    private readonly IDiscordIntegrationService _discordIntegration;
    private readonly SplashScreenService _splashScreenService;
    private readonly IThemeService _themes;
    private readonly ITranslationService _translation;

    public MainWindow()
    {
        InitializeComponent();
        _configs = null!;
        _appSettings = null!;
        _translation = null!;
        _themes = null!;
        _discordIntegration = null!;
        _splashScreenService = null!;
    }

    public MainWindow(
        MainWindowViewModel viewModel,
        MainMenuViewModel menuViewModel,
        IConfigService configs,
        AppSettings appSettings,
        IThemeService themes,
        ITranslationService translation,
        DragDropService dragDropService,
        ShortcutsService shortcutsService,
        IDiscordIntegrationService discordIntegration,
        SplashScreenService splashScreenService)
        : this()
    {
        DataContext = viewModel;
        _configs = configs;
        _appSettings = appSettings;
        _translation = translation;
        _themes = themes;
        _discordIntegration = discordIntegration;
        _splashScreenService = splashScreenService;

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
        Closing += async (_, _) => await OnClosing();
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

    public async Task OnClosing()
    {
        StateConfig? stateConfig = _configs.GetConfig<StateConfig>();

        stateConfig?.MainWindowHeight = Height;
        stateConfig?.MainWindowWidth = Width;
        stateConfig?.MainWindowX = Position.X;
        stateConfig?.MainWindowY = Position.Y;
        stateConfig?.MainWindowMaximized = WindowState == WindowState.Maximized;

        await _configs.SaveAllConfigsImmediateAsync();
    }

    public async Task OnLoaded()
    {
        await _translation.ChangeLanguageAsync(_appSettings.Language, true);
        await SplashScreen.TryUpdateStatusAsync("Loading themes and addons...");

        await _themes.SetThemeAsync(_themes.CurrentTheme);

        App.Stopwatch.Stop();
        _splashScreenService.UpdateStartupTime((int)App.Stopwatch.ElapsedMilliseconds);

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
}