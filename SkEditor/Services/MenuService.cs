using System;
using System.Collections.Generic;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Menu;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services;

public class MenuService(IFileService files, ITabService tabs, Func<SettingsTab> settingsTabFactory) : IMenuService
{
    public IEnumerable<MainMenuItem> GetMainMenuItems()
    {
        List<MainMenuItem> items =
        [
            BuildFileMenu(),
        ];

        MainMenuItem settings =
            MainMenuItem.IconOnly(Symbol.Settings).WithCommand(() => tabs.OpenUniqueTab(settingsTabFactory));

        items.Add(settings);

        return items;
    }

    private MainMenuItem BuildFileMenu()
    {
        MainMenuItem fileMenu = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderFile);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderNew, Symbol.Document)
            .WithCommand(files.NewFile)
            .WithInputGesture("Ctrl+N")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpen, Symbol.DocumentArrowUp)
            .WithCommand(files.OpenFileAsync)
            .WithInputGesture("Ctrl+O")
            .AddTo(fileMenu);

        MainMenuItem.Separator.AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSave, Symbol.Save)
            .WithCommand(files.SaveFileAsync)
            .WithInputGesture("Ctrl+S")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAs, Symbol.SaveEdit)
            .WithCommand(files.SaveAsFileAsync)
            .WithInputGesture("Ctrl+Shift+S")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAll, Symbol.SaveMultiple)
            .WithCommand(files.SaveAllAsync)
            .WithInputGesture("Ctrl+Alt+S")
            .AddTo(fileMenu);

        return fileMenu;
    }
}