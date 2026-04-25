using System.Threading.Tasks;
using FluentAvalonia.UI.Windowing;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Localization;
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
    private readonly IThemeService _themes;
    private readonly ITranslationService _translation;

    public MainWindow()
    {
        InitializeComponent();
        _appSettings = null!;
        _translation = null!;
        _themes = null!;
    }

    public MainWindow(
        MainWindowViewModel viewModel,
        MainMenuViewModel menuViewModel,
        AppSettings appSettings,
        IThemeService themes,
        ITranslationService translation)
        : this()
    {
        DataContext = viewModel;
        _appSettings = appSettings;
        _translation = translation;
        _themes = themes;

        KeyboardShortcutHandler shortcutHandler = new(this);
        shortcutHandler.Initialize();

        menuViewModel.RegisterHotKeys(this);

        WindowStyler.Style(this);
        _themes.LoadThemes();

        Loaded += async (_, _) => await OnLoaded();
    }

    public new SplashScreen? SplashScreen { get; set; }

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