using System.Threading.Tasks;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly ILogService _log;
    private readonly KeyboardShortcutHandler? _shortcutHandler;
    private readonly ITranslationService _translation;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
        _log = App.Services.GetRequiredService<ILogService>();
        _appSettings = App.Services.GetRequiredService<AppSettings>();
        _translation = App.Services.GetRequiredService<ITranslationService>();

        _shortcutHandler = new KeyboardShortcutHandler(this);
        _shortcutHandler.Initialize();
        App.Services.GetRequiredService<MainMenuViewModel>().RegisterHotKeys(this);

        WindowStyler.Style(this);

        Loaded += async (_, _) => await OnLoaded();
    }

    public SplashScreen? SplashScreen { get; set; }

    public async Task OnLoaded()
    {
        await _translation.ChangeLanguageAsync(_appSettings.Language, true);
        await SplashScreen.TryUpdateStatusAsync("Loading themes and addons...");

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