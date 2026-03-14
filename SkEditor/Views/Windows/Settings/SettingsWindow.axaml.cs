using System;
using Avalonia.Controls;
using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities.Styling;
using SkEditor.ViewModels.Pages;
using SkEditor.Views.Pages.Settings;
using SkEditor.Views.Pages.Settings.Personalization;

namespace SkEditor.Views.Windows.Settings;

public partial class SettingsWindow : AppWindow
{
    public SettingsWindow()
    {
        InitializeComponent();
        Focusable = true;
        Instance = this;
    }

    public SettingsWindow(ThemePageViewModel themePageViewModel) : this()
    {
        WindowStyler.Style(this);
        TitleBar.ExtendsContentIntoTitleBar = false;

        SkEditorAPI.Events.SettingsOpened();

        FrameView.NavigationPageFactory = new SettingsPageFactory(themePageViewModel);

        KeyDown += (_, e) =>
        {
            if (e.Key != Key.Escape)
            {
                return;
            }

            if (FrameView.CanGoBack)
            {
                FrameView.GoBack();
            }
            else
            {
                Close();
            }
        };
        Closed += (_, _) => { _ = SkEditorAPI.Configs.SaveAllConfigsAsync(); };
    }

    public static SettingsWindow Instance { get; private set; } = null!;

    public Frame GetFrameView()
    {
        return FrameView;
    }

    public static void NavigateToPage(Type page)
    {
        EntranceNavigationTransitionInfo transitionInfo = new();

        FrameNavigationOptions navOpt = new()
            { TransitionInfoOverride = transitionInfo, IsNavigationStackEnabled = true };
        Instance.FrameView.NavigateToType(page, null, navOpt);

        if (page == typeof(HomePage))
        {
            Instance.FrameView.BackStack.Clear();
            Instance.FrameView.ForwardStack.Clear();
        }
        else if (Instance.FrameView.BackStack.Count == 0)
        {
            Instance.FrameView.BackStack.Add(new PageStackEntry(typeof(HomePage), null, transitionInfo));
        }
    }
}

public class SettingsPageFactory(ThemePageViewModel themePageViewModel) : INavigationPageFactory
{
    public Control? GetPage(Type sourcePageType)
    {
        if (sourcePageType == typeof(HomePage))
        {
            return new HomePage();
        }

        if (sourcePageType == typeof(GeneralPage))
        {
            return new GeneralPage();
        }

        if (sourcePageType == typeof(PersonalizationPage))
        {
            return new PersonalizationPage();
        }

        if (sourcePageType == typeof(ExperimentsPage))
        {
            return new ExperimentsPage();
        }

        if (sourcePageType == typeof(AboutPage))
        {
            return new AboutPage();
        }

        if (sourcePageType == typeof(AddonsPage))
        {
            return new AddonsPage();
        }

        if (sourcePageType == typeof(ConnectionsPage))
        {
            return new ConnectionsPage();
        }

        if (sourcePageType == typeof(FileTypesPage))
        {
            return new FileTypesPage();
        }

        if (sourcePageType == typeof(CustomAddonSettingsPage))
        {
            return new CustomAddonSettingsPage();
        }

        if (sourcePageType == typeof(EditThemePage))
        {
            return new EditThemePage();
        }

        if (sourcePageType == typeof(ThemePage))
        {
            return new ThemePage(themePageViewModel);
        }

        if (sourcePageType == typeof(FileSyntaxes))
        {
            return new FileSyntaxes();
        }

        return null;
    }

    public Control? GetPageFromObject(object target)
    {
        return null;
    }
}