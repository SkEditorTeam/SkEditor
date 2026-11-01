using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using HotAvalonia;
using SkEditor.Appearance.Themes;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Localization;
using SkEditor.Core.Updates;
using SkEditor.Core.Updates.Models;
using SkEditor.Editor.Syntax;
using SkEditor.Integrations.Discord;
using SkEditor.Layout.Menu.UI;
using SkEditor.Windowing.Errors;

namespace SkEditor.Windowing.UI;

public partial class MainWindow : Window
{
    private readonly AppSettings _appSettings;
    private readonly IDiscordIntegrationService _discordIntegration;
    private readonly SplashScreenService _splashScreen;
    private readonly IThemeService _themes;
    private readonly ITranslationService _translation;
    private readonly IUpdateService _updates;
    private readonly IWindowCloseService _windowClose;
    private readonly ICrashService _crashes;

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
        _crashes = null!;
        _updates = null!;
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
        SplashScreenService splashScreen,
        ICrashService crashes,
        IUpdateService updates)
        : this()
    {
        DataContext = viewModel;
        _appSettings = appSettings;
        _translation = translation;
        _themes = themes;
        _discordIntegration = discordIntegration;
        _splashScreen = splashScreen;
        _windowClose = windowClose;
        _crashes = crashes;
        _updates = updates;

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

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            IsVisible = true;
            Activate();
            Focus();

            SplashScreen?.Close();
        });
        
        await Task.Delay(100);
        await _crashes.CheckForCrashes();

        if (_appSettings.CheckForUpdates)
        {
            await Task.Delay(1000);
        
            ReleaseInfo? update = await _updates.CheckForUpdates();
            if (update is not null)
            {
                _updates.ShowNotification(update);
            }   
        }
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
            await TryCloseAsync();
        };
        dragDropService.AttachDragDropEvents(this);
        shortcutsService.AttachKeyEvents(this);
    }
    
    public async Task<bool> TryCloseAsync()
    {
        bool canClose = await _windowClose.OnWindowClosing(Height, Width, Position.X, Position.Y, WindowState);
        if (canClose)
        {
            Close();
            return true;
        }

        return false;
    }
}