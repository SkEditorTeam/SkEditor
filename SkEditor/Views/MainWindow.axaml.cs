using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Controls;
using SkEditor.Utilities;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace SkEditor.Views;

public partial class MainWindow : AppWindow
{
    public static MainWindow Instance { get; private set; }

    public BottomBarControl GetBottomBar() => this.FindControl<BottomBarControl>("BottomBar");

    public MainWindow()
    {
        InitializeComponent();

        WindowStyler.Style(this);
        ThemeEditor.LoadThemes();
        AddEvents();

        Translation.LoadDefaultLanguage();
        Translation.ChangeLanguage(ApiVault.Get().GetAppConfig().Language);

        Instance = this;
    }

    private void AddEvents()
    {
        TabControl.AddTabButtonCommand = new RelayCommand(FileHandler.NewFile);
        TabControl.TabCloseRequested += (sender, e) => FileCloser.CloseFile(e);
        TabControl.SelectionChanged += (_, _) => SideBar.ParserPanel.Panel.ParseCurrentFile();
        TemplateApplied += OnWindowLoaded;
        Closing += OnClosing;

        Activated += (sender, e) => ChangeChecker.Check();

        KeyDown += (sender, e) =>
        {
            if (e.KeyModifiers == KeyModifiers.Control && e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                FileHandler.SwitchTab((int)e.Key - 35);
            }
        };

        DragDrop.SetAllowDrop(this, true);
        DragDrop.DropEvent.AddClassHandler(FileHandler.FileDropAction);
    }

    public bool AlreadyClosed { get; set; } = false;
    private async void OnClosing(object sender, WindowClosingEventArgs e)
    {
        if (AlreadyClosed) return;

        ThemeEditor.SaveAllThemes();
        ApiVault.Get().GetAppConfig().Save();

        e.Cancel = true;
        if (!ApiVault.Get().GetAppConfig().EnableSessionRestoring)
        {
            List<TabViewItem> unsavedFiles = ApiVault.Get().GetTabView().TabItems.Cast<TabViewItem>().Where(item => item.Header.ToString().EndsWith('*')).ToList();
            if (unsavedFiles.Count == 0) return;

            ContentDialogResult result = await ApiVault.Get().ShowMessageWithIcon(Translation.Get("Attention"), Translation.Get("ClosingProgramWithUnsavedFiles"), new SymbolIconSource() { Symbol = Symbol.ImportantFilled });
            if (result == ContentDialogResult.Primary)
            {
                unsavedFiles.ForEach(item => item.Header = item.Header.ToString().TrimEnd('*'));
                ApiVault.Get().OnClosed();
                Close();
            }
        }
        else
        {
            SessionRestorer.SaveSession();
        }
    }

    private async void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        AddonLoader.Load();
        Utilities.Files.FileTypes.RegisterDefaultAssociations();
        SideBar.IsVisible = MainMenu.MenuItemOpenFolder.IsVisible = ApiVault.Get().GetAppConfig().EnableProjectsExperiment;
        SideBar.LoadPanels();

        await ThemeEditor.SetTheme(ThemeEditor.CurrentTheme);

        bool sessionFilesAdded = false;
        if (ApiVault.Get().GetAppConfig().EnableSessionRestoring) sessionFilesAdded = await SessionRestorer.RestoreSession();

        string[] startupFiles = ApiVault.Get().GetStartupFiles();
        if (startupFiles.Length == 0 && !await CrashChecker.CheckForCrash() && !sessionFilesAdded) FileHandler.NewFile();
        startupFiles.ToList().ForEach(FileHandler.OpenFile);

        Dispatcher.UIThread.Post(() =>
        {
            SyntaxLoader.LoadAdvancedSyntaxes();
            DiscordRpcUpdater.Initialize();

            if (ApiVault.Get().GetAppConfig().CheckForUpdates) UpdateChecker.Check();

            Tutorial.ShowTutorial();
            BottomBar.UpdatePosition();
            ChangelogChecker.Check();
        });
    }
}