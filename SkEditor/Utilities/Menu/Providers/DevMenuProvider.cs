using FluentIcons.Common;
using R3;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Controls;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

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