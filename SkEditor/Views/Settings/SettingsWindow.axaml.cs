using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;
using FluentAvalonia.UI.Windowing;
using SkEditor.API;
using SkEditor.Utilities.Styling;
using System;

namespace SkEditor.Views;
public partial class SettingsWindow : AppWindow
{
    public static SettingsWindow Instance { get; private set; }

    public Frame GetFrameView() => FrameView;

    public SettingsWindow()
    {
        InitializeComponent();

        WindowStyler.Style(this);
        TitleBar.ExtendsContentIntoTitleBar = false;

        Instance = this;
        (SkEditorAPI.Events as Events).SettingsOpened();
    }

    public static void NavigateToPage(Type page)
    {
        var navOpt = new FrameNavigationOptions() { TransitionInfoOverride = new EntranceNavigationTransitionInfo() };
        Instance.FrameView.NavigateToType(page, null, navOpt);
    }
}
