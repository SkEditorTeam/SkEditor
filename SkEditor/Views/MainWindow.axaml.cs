using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.InternalAPI;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Views;

public partial class MainWindow : AppWindow
{
    public static MainWindow Instance { get; private set; }
    private readonly SplashScreen? _splashScreen;
    private bool _isFullyLoaded;

    public BottomBarControl GetBottomBar() => BottomBar;

    public MainWindow(SplashScreen? splashScreen = null)
    {
        _splashScreen = splashScreen;

        InitializeComponent();

        WindowStyler.Style(this);
        TitleBar.Height = 50;

        ThemeEditor.LoadThemes();
        AddEvents();

        Translation.ChangeLanguage(SkEditorAPI.Core.GetAppConfig().Language).Wait();

        Instance = this;
    }

    private void AddEvents()
    {
        TabControl.AddTabButtonCommand = new RelayCommand(FileHandler.NewFile);
        TabControl.TabCloseRequested += async (_, e) => await FileCloser.CloseFile(e);
        TabControl.SelectionChanged += (_, e) => SkEditorAPI.Events.TabChanged(e);
        TemplateApplied += OnWindowLoaded;
        Closing += OnClosing;

        Activated += (_, _) => _ = ChangeChecker.Check();

        KeyDown += (_, e) =>
        {
            switch (e)
            {
                case { KeyModifiers: KeyModifiers.Control, Key: >= Key.D1 and <= Key.D9 }:
                    FileHandler.SwitchTab((int)e.Key - 35);
                    break;
                case { KeyModifiers: (KeyModifiers.Control | KeyModifiers.Shift), Key: Key.Oem3 }:
                {
                    TerminalWindow terminal = new();
                    terminal.Show();
                    break;
                }
            }
        };

        AddHandler(DragDrop.DropEvent, FileHandler.FileDropAction);
    }

    public void ReloadUiOfAddons()
    {
        MainMenu.ReloadAddonsMenus();
        BottomBar.ReloadBottomIcons();
        SideBar.ReloadPanels();
    }

    public bool AlreadyClosed { get; set; }

    private async void OnClosing(object sender, WindowClosingEventArgs e)
    {
        try
        {
            if (AlreadyClosed) return;

            ThemeEditor.SaveAllThemes();
            SkEditorAPI.Core.GetAppConfig().Save();

            e.Cancel = true;
            if (!SkEditorAPI.Core.GetAppConfig().EnableSessionRestoring)
            {
                bool anyUnsaved = SkEditorAPI.Files.GetOpenedEditors().Any(x => !x.IsSaved);
                if (!anyUnsaved)
                {
                    e.Cancel = false;
                    return;
                }

                ContentDialogResult result = await SkEditorAPI.Windows.ShowDialog(Translation.Get("Attention"),
                    Translation.Get("ClosingProgramWithUnsavedFiles"), icon: Symbol.ImportantFilled,
                    primaryButtonText: "Yes", cancelButtonText: "No");

                if (result != ContentDialogResult.Primary) return;

                AlreadyClosed = true;
                Close();
            }
            else
            {
                await SessionRestorer.SaveSession();
                AlreadyClosed = true;
                Close();
            }
        }
        catch (Exception exc)
        {
            SkEditorAPI.Logs.Error($"Error while closing the window: {exc.Message}");
            AlreadyClosed = true;
            Close();
        }
        finally
        {
            SkEditorAPI.Core.GetAppConfig().Save();
        }
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _splashScreen?.UpdateStatus("Loading themes and addons...");

            var themeTask = ThemeEditor.SetTheme(ThemeEditor.CurrentTheme);
            var addonTask = AddonLoader.Load();
            await Task.WhenAll(themeTask, addonTask);

            _splashScreen?.UpdateStatus("Registering file types...");
            Utilities.Files.FileTypes.RegisterDefaultAssociations();
            SideBar.ReloadPanels();

            double scale = SkEditorAPI.Core.GetAppConfig().CustomUiScale;
            LayoutTransform.LayoutTransform = new ScaleTransform(scale, scale);
            
            bool sessionFilesAdded = false;
            if (SkEditorAPI.Core.GetAppConfig().EnableSessionRestoring)
            {
                _splashScreen?.UpdateStatus("Restoring session...");
                sessionFilesAdded = await SessionRestorer.RestoreSession();
            }

            _splashScreen?.UpdateStatus("Opening files...");
            string[] startupFiles = SkEditorAPI.Core.GetStartupArguments();
            if (startupFiles.Length == 0 && !sessionFilesAdded)
                SkEditorAPI.Files.AddWelcomeTab();
            startupFiles.ToList().ForEach(FileHandler.OpenFile);
            if (SkEditorAPI.Files.GetOpenedFiles().Count == 0)
                SkEditorAPI.Files.AddWelcomeTab();

            BottomBar.UpdatePosition();

            _splashScreen?.UpdateStatus("Finishing up...");

            await Task.Run(() =>
            {
                try
                {
                    SyntaxLoader.LoadAdvancedSyntaxes();
                    DiscordRpcUpdater.Initialize();
                }
                catch (Exception exc)
                {
                    SkEditorAPI.Logs.Error($"Error loading final components: {exc.Message}", true);
                }
            });

            _isFullyLoaded = true;

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
                        if (SkEditorAPI.Core.GetAppConfig().CheckForUpdates)
                            UpdateChecker.Check();

                        Tutorial.ShowTutorial();
                        ChangelogChecker.Check();

                        await CrashChecker.CheckForCrash();
                    }
                    catch (Exception exc)
                    {
                        SkEditorAPI.Logs.Error($"Error loading non-blocking components: {exc.Message}", true);
                    }
                }, DispatcherPriority.Background);
            }, DispatcherPriority.Background);
        }
        catch (Exception exc)
        {
            SkEditorAPI.Logs.Error($"Something went wrong while loading the window: {exc.Message}", true);

            _isFullyLoaded = true;
            IsVisible = true;
            _splashScreen?.Close();
        }
    }

    public bool IsFullyLoaded() => _isFullyLoaded;
}