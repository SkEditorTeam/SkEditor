using FluentIcons.Common;
using R3;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Localization;
using SkEditor.Layout.Menu.Models;
using SkEditor.Layout.StatusBar.UI;

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