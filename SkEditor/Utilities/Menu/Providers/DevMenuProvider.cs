using FluentIcons.Common;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Controls;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

public class DevMenuProvider(AppSettings appSettings, IThemeService themes, BottomDrawerViewModel bottomDrawer)
    : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem devMenu = MainMenuItem.HeaderOnly(TranslatableString.FromRawString("Dev"))
            .WithVisibility(appSettings.ObservePropertyChanged(x => x.IsDevModeEnabled).ToBindableReactiveProperty());

        MainMenuItem.HeaderWithIcon(TranslatableString.FromRawString("Reload Themes"), Symbol.Color)
            .WithCommand(themes.ReloadThemesAsync)
            .AddTo(devMenu);

        MainMenuItem.HeaderWithIcon("Logs".ToTranslatable(), Symbol.List)
            .WithCommand(() => bottomDrawer.TogglePane("Logs"))
            .AddTo(devMenu);

        return devMenu;
    }
}