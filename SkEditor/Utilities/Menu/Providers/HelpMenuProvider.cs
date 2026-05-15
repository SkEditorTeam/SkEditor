using FluentIcons.Common;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Utilities.Localization;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

public class HelpMenuProvider(IWindowDialogService windowDialogs) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem helpMenu = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderHelp);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderAbout, Symbol.Info)
            .WithCommand(windowDialogs.ShowAboutDialogAsync)
            .AddTo(helpMenu);

        return helpMenu;
    }
}