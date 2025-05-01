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

namespace SkEditor.Views;

public partial class MainWindow : AppWindow
{
    public static MainWindow Instance { get; private set; }

    public BottomBarControl GetBottomBar() => BottomBar;

    public MainWindow()
    {
        InitializeComponent();

        WindowStyler.Style(this);
        TitleBar.Height = 50;

        ThemeEditor.LoadThemes();
        AddEvents();

        Translation.LoadDefaultLanguage();
        Translation.ChangeLanguage(SkEditorAPI.Core.GetAppConfig().Language);

        Instance = this;
    }

    private void AddEvents()
    {
        TabControl.AddTabButtonCommand = new RelayCommand(FileHandler.NewFile);
        TabControl.TabCloseRequested += (_, e) => FileCloser.CloseFile(e);
        TabControl.SelectionChanged += (_, e) => SkEditorAPI.Events.TabChanged(e);
        TemplateApplied += OnWindowLoaded;
        Closing += OnClosing;

        Activated += (_, _) => ChangeChecker.Check();

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

                if (result == ContentDialogResult.Primary)
                {
                    AlreadyClosed = true;
                    Close();
                }
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
            AddonLoader.Load();
            Utilities.Files.FileTypes.RegisterDefaultAssociations();
            SideBar.ReloadPanels();

            await ThemeEditor.SetTheme(ThemeEditor.CurrentTheme);

            double scale = SkEditorAPI.Core.GetAppConfig().CustomUiScale;
            LayoutTransform.LayoutTransform = new ScaleTransform(scale, scale);

            bool sessionFilesAdded = false;
            if (SkEditorAPI.Core.GetAppConfig().EnableSessionRestoring)
                sessionFilesAdded = await SessionRestorer.RestoreSession();

            string[] startupFiles = SkEditorAPI.Core.GetStartupArguments();
            if (startupFiles.Length == 0 && !sessionFilesAdded)
                SkEditorAPI.Files.AddWelcomeTab();
            startupFiles.ToList().ForEach(FileHandler.OpenFile);
            if (SkEditorAPI.Files.GetOpenedFiles().Count == 0)
                SkEditorAPI.Files.AddWelcomeTab();

            Dispatcher.UIThread.Post(async void () =>
            {
                try
                {
                    SyntaxLoader.LoadAdvancedSyntaxes();
                    DiscordRpcUpdater.Initialize();

                    if (SkEditorAPI.Core.GetAppConfig().CheckForUpdates) UpdateChecker.Check();

                    Tutorial.ShowTutorial();
                    BottomBar.UpdatePosition();
                    ChangelogChecker.Check();

                    await CrashChecker.CheckForCrash();
                }
                catch (Exception exc)
                {
                    SkEditorAPI.Logs.Error($"Something went wrong while loading the window: {exc.Message}", true);
                }
            });
        }
        catch (Exception exc)
        {
            SkEditorAPI.Logs.Error($"Something went wrong while loading the window: {exc.Message}", true);
        }
    }
}