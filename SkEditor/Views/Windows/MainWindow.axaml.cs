using System.Threading.Tasks;
using Avalonia.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Input;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Startup;
using SkEditor.Utilities.Styling;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Windows;

namespace SkEditor.Views.Windows;

public partial class MainWindow : FAAppWindow
{
    private readonly AppSettings _appSettings;
    private readonly IConfigService _configs;
    private readonly IThemeService _themes;
    private readonly ITranslationService _translation;

    public MainWindow()
    {
        InitializeComponent();
        _configs = null!;
        _appSettings = null!;
        _translation = null!;
        _themes = null!;
    }

    public MainWindow(
        MainWindowViewModel viewModel,
        MainMenuViewModel menuViewModel,
        IConfigService configs,
        AppSettings appSettings,
        IThemeService themes,
        ITranslationService translation,
        DragDropService dragDropService)
        : this()
    {
        DataContext = viewModel;
        _configs = configs;
        _appSettings = appSettings;
        _translation = translation;
        _themes = themes;

        KeyboardShortcutHandler shortcutHandler = new(this);
        shortcutHandler.Initialize();

        menuViewModel.RegisterHotKeys(this);

        WindowStyler.Style(this);
        _themes.LoadThemes();

        AttachEvents(dragDropService);
    }

    public new SplashScreen? SplashScreen { get; set; }

    private void AttachEvents(DragDropService dragDropService)
    {
        Loaded += async (_, _) => await OnLoaded();
        Closing += async (_, _) => await OnClosing();
        dragDropService.AttachDragDropEvents(this);
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
        SplashScreenHandler.UpdateStartupTime((int)App.Stopwatch.ElapsedMilliseconds);

        await SplashScreen.TryUpdateStatusAsync("Initializing main window...");

        Dispatcher.Post(() =>
        {
            IsVisible = true;
            Activate();
            Focus();

            SplashScreen?.Close();
        });
    }
}