using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.API;
using SkEditor.Contracts.Services;
using SkEditor.Services;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Files.Session;
using SkEditor.Utilities.Input;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Startup;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using SkEditor.Utilities.Updating;
using SkEditor.ViewModels.Controls;
using SkEditor.ViewModels.Windows;
using DiscordRpcUpdater = SkEditor.Utilities.Discord.DiscordRpcUpdater;
using Symbol = FluentIcons.Common.Symbol;

namespace SkEditor.Views.Windows;

public partial class MainWindow : AppWindow
{
    private readonly ILogService _log;
    private readonly KeyboardShortcutHandler? _shortcutHandler;

    private bool _isFullyLoaded;
    private SplashScreen? _splashScreen;

    public MainWindow()
    {
        InitializeComponent();

        DataContext = App.Services.GetRequiredService<MainWindowViewModel>();
        _log = App.Services.GetRequiredService<ILogService>();

        _shortcutHandler = new KeyboardShortcutHandler(this);
        _shortcutHandler.Initialize();
        App.Services.GetRequiredService<MainMenuViewModel>().RegisterHotKeys(this);

        WindowStyler.Style(this);

        ThemeService.ServiceStatic.LoadThemes();
        AddEvents();
    }

    public bool AlreadyClosed { get; set; }

    private void AddEvents()
    {
        TabControl.AddTabButtonCommand = new RelayCommand(FileHandler.NewFile);
        TabControl.TabCloseRequested += async (_, e) => await FileCloser.CloseFile(e);
        TabControl.SelectionChanged += (_, e) => SkEditorAPI.Events.TabChanged(e);
        TemplateApplied += OnWindowLoaded;
        Closing += OnClosing;

        Activated += (_, _) => _ = ChangeChecker.Check();

        AddHandler(DragDrop.DropEvent, FileHandler.FileDropAction);
    }

    public void ReloadUiOfAddons()
    {
        BottomBar.ReloadBottomIcons();
        SideBar.ReloadPanels();
    }

    private async void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        try
        {
            if (AlreadyClosed)
            {
                return;
            }

            StateConfig stateConfig = SkEditorAPI.Configs.GetStateConfig();

            stateConfig.MainWindowHeight = Height;
            stateConfig.MainWindowWidth = Width;
            stateConfig.MainWindowX = Position.X;
            stateConfig.MainWindowY = Position.Y;
            stateConfig.MainWindowMaximized = WindowState == WindowState.Maximized;

            ThemeService.ServiceStatic.SaveAllThemes();
            await SkEditorAPI.Configs.SaveAllConfigsImmediateAsync();

            e.Cancel = true;
            if (!SkEditorAPI.Configs.GetExperimentsConfig().SessionRestoringEnabled)
            {
                bool anyUnsaved = SkEditorAPI.Files.GetOpenedEditors().Any(x => !x.IsSaved);
                if (!anyUnsaved)
                {
                    e.Cancel = false;
                    return;
                }

                if (await PromptForUnsavedChanges())
                {
                    return;
                }

                AlreadyClosed = true;
                Close();
            }
            else
            {
                bool isSuccess = await SessionRestorer.SaveSession();
                if (!isSuccess)
                {
                    if (await PromptForUnsavedChanges())
                    {
                        return;
                    }
                }

                AlreadyClosed = true;
                Close();
            }
        }
        catch (Exception exc)
        {
            _log.Error($"Error while closing the window: {exc.Message}");
            AlreadyClosed = true;
            Close();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        _shortcutHandler?.Dispose();
        base.OnClosed(e);
    }

    private static async Task<bool> PromptForUnsavedChanges()
    {
        ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog(Translation.Get(TranslationKey.Attention),
            Translation.Get(TranslationKey.ClosingProgramWithUnsavedFiles), Symbol.Warning,
            primaryButtonText: "Yes", cancelButtonText: "No");

        return result != ContentDialogResult.Primary;
    }

    private async void OnWindowLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            await Translation.ChangeLanguage(SkEditorAPI.Configs.GetAppSettings().Language, true);
            await _splashScreen.TryUpdateStatusAsync("Loading themes and addons...");

            Task themeTask = ThemeService.ServiceStatic.SetTheme(ThemeService.ServiceStatic.CurrentTheme);
            Task addonTask = AddonLoader.Load();
            await Task.WhenAll(themeTask, addonTask);

            await _splashScreen.TryUpdateStatusAsync("Registering file types...");
            Utilities.Files.FileTypes.RegisterDefaultAssociations();
            SideBar.ReloadPanels();

            double scale = SkEditorAPI.Configs.GetAppSettings().CustomUiScale;
            LayoutTransform.LayoutTransform = new ScaleTransform(scale, scale);

            bool sessionFilesAdded = false;
            if (SkEditorAPI.Configs.GetExperimentsConfig().SessionRestoringEnabled)
            {
                await _splashScreen.TryUpdateStatusAsync("Restoring session...");
                sessionFilesAdded = await SessionRestorer.RestoreSession();
            }

            await _splashScreen.TryUpdateStatusAsync("Opening files...");
            string[] startupFiles = SkEditorAPI.Core.GetStartupArguments();
            if (startupFiles.Length == 0 && !sessionFilesAdded)
            {
                SkEditorAPI.Files.AddWelcomeTab();
            }

            startupFiles.ToList().ForEach(FileHandler.OpenFile);
            if (SkEditorAPI.Files.GetOpenedFiles().Count == 0)
            {
                SkEditorAPI.Files.AddWelcomeTab();
            }

            BottomBar.UpdatePosition();

            await _splashScreen.TryUpdateStatusAsync("Finishing up...");

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    await SyntaxLoader.LoadAdvancedSyntaxes();
                    DiscordRpcUpdater.Initialize();
                }
                catch (Exception exc)
                {
                    _log.Error($"Error loading final components: {exc.Message}", true);
                }
            });

            _isFullyLoaded = true;

            App.Stopwatch.Stop();
            SplashScreenHandler.UpdateStartupTime((int)App.Stopwatch.ElapsedMilliseconds);

            Dispatcher.UIThread.Post(() =>
            {
                IsVisible = true;
                Activate();
                Focus();

                _splashScreen?.Close();

                Dispatcher.UIThread.Post(async void () =>
                {
                    try
                    {
                        if (SkEditorAPI.Configs.GetAppSettings().CheckForAppUpdates)
                        {
                            await UpdateChecker.Check();
                        }

                        await Tutorial.ShowTutorial();
                        await ChangelogChecker.Check();

                        await App.Services.GetRequiredService<CrashChecker>().CheckForCrash();

                        await AddonUpdateChecker.CheckForUpdates();
                    }
                    catch (Exception exc)
                    {
                        _log.Error($"Error loading non-blocking components: {exc.Message}", true);
                    }
                }, DispatcherPriority.Background);
            }, DispatcherPriority.Background);
        }
        catch (Exception exc)
        {
            _log.Error(
                $"Something went wrong while loading the window: {exc.Message} + {exc.InnerException} {exc.StackTrace}",
                true);

            _isFullyLoaded = true;
            IsVisible = true;
            _splashScreen?.Close();
        }
    }

    public bool IsFullyLoaded()
    {
        return _isFullyLoaded;
    }

    public void SetSplashScreen(SplashScreen? splashScreen)
    {
        if (_splashScreen != null)
        {
            throw new InvalidOperationException("Splash screen has already been set.");
        }

        _splashScreen = splashScreen;
    }
}