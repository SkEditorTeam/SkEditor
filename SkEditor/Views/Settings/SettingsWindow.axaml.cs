using Avalonia.Input;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.Styling;
using SkEditor.Views.Settings;
using System;

namespace SkEditor.Views;
public partial class SettingsWindow : AppWindow
{
    public static SettingsWindow Instance { get; private set; }

    public Frame GetFrameView() => FrameView;

    public SettingsWindow()
    {
        InitializeComponent();
        Focusable = true;

        WindowStyler.Style(this);
        TitleBar.ExtendsContentIntoTitleBar = false;

        Instance = this;
        SkEditorAPI.Events.SettingsOpened();

        KeyDown += (_, e) =>
        {
            if (e.Key != Key.Escape) return;

            if (FrameView.CanGoBack)
            {
                FrameView.GoBack();
            }
            else
            {
                Close();
            }
        };
        Closed += (s, e) => SkEditorAPI.Core.GetAppConfig().Save();
    }

    public static void NavigateToPage(Type page)
    {
        EntranceNavigationTransitionInfo transitionInfo = new();

        var navOpt = new FrameNavigationOptions() { TransitionInfoOverride = transitionInfo, IsNavigationStackEnabled = true };
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
