using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

public class DevMenuProvider(AppSettings appSettings, IThemeService themes) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem devMenu = MainMenuItem.HeaderOnly(TranslatableString.FromRawString("Dev"));

        MainMenuItem.HeaderWithIcon(TranslatableString.FromRawString("Reload Theme"), Symbol.Color)
            .WithCommand(themes.ReloadCurrentThemeAsync)
            .AddTo(devMenu);

        devMenu.IsVisible = appSettings.IsDevModeEnabled;
        appSettings.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AppSettings.IsDevModeEnabled))
            {
                devMenu.IsVisible = appSettings.IsDevModeEnabled;
            }
        };

        return devMenu;
    }
}