using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using HotAvalonia;
using SkEditor.Appearance.Themes;
using SkEditor.Core.Localization;
using SkEditor.Editor.Syntax;
using SkEditor.Integrations.Discord;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;
using MainMenuViewModel = SkEditor.Layout.Menu.UI.MainMenuViewModel;

namespace SkEditor.Windowing.UI;

public partial class MainWindow : Window
{
    private readonly AppSettings _appSettings;
    private readonly IDiscordIntegrationService _discordIntegration;
    private readonly SplashScreenService _splashScreen;
    private readonly IThemeService _themes;
    private readonly ITranslationService _translation;
    private readonly IWindowCloseService _windowClose;

    public SplashScreen? SplashScreen { get; set; }

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
        ISyntaxService syntaxes,
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

        RegisterHotKeys(menuViewModel, appSettings);

        _themes.LoadThemes();
        syntaxes.LoadSyntaxes();

        LayoutTransform.LayoutTransform = new ScaleTransform(appSettings.CustomUiScale, appSettings.CustomUiScale);

        AttachEvents(dragDropService, shortcutsService);
    }

    public async Task OnLoaded()
    {
        await _translation.ChangeLanguageAsync(_appSettings.Language);
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

    private void RegisterHotKeys(MainMenuViewModel menuViewModel, AppSettings appSettings)
    {
        if (!OperatingSystem.IsMacOS() || appSettings.UseIntegratedMenuOnMacOs)
        {
            menuViewModel.RegisterHotKeys(this);
        }
    }

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
}