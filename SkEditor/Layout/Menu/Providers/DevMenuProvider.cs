using FluentIcons.Common;
using R3;
using SkEditor.Core.Localization;
using SkEditor.Layout.StatusBar.UI;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;
using MainMenuItem = SkEditor.Layout.Menu.Models.MainMenuItem;

namespace SkEditor.Layout.Menu.Providers;

public class DevMenuProvider(AppSettings appSettings, BottomDrawerViewModel bottomDrawer)
    : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem devMenu = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderDev)
            .WithVisibility(appSettings.ObservePropertyChanged(x => x.IsDevModeEnabled).ToBindableReactiveProperty());

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderDevLogs, Symbol.List)
            .WithCommand(() => bottomDrawer.TogglePane("Logs"))
            .AddTo(devMenu);

        return devMenu;
    }
}