using System.Collections.Generic;
using FluentIcons.Common;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Menu;
using SkEditor.Utilities.Localization;

namespace SkEditor.Services;

public class MenuService(IFileService fileService) : IMenuService
{
    public IEnumerable<MainMenuItem> GetMainMenuItems()
    {
        List<MainMenuItem> items =
        [
            BuildFileMenu(),
        ];

        return items;
    }

    private MainMenuItem BuildFileMenu()
    {
        MainMenuItem fileMenu = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderFile);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderNew, Symbol.Document)
            .WithCommand(fileService.NewFile)
            .WithInputGesture("Ctrl+N")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpen, Symbol.DocumentArrowUp)
            .WithCommand(fileService.OpenFileAsync)
            .WithInputGesture("Ctrl+O")
            .AddTo(fileMenu);

        MainMenuItem.Separator.AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSave, Symbol.Save)
            .WithCommand(fileService.SaveFileAsync)
            .WithInputGesture("Ctrl+S")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAs, Symbol.SaveEdit)
            .WithCommand(fileService.SaveAsFileAsync)
            .WithInputGesture("Ctrl+Shift+S")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAll, Symbol.SaveMultiple)
            .WithCommand(fileService.SaveAllAsync)
            .WithInputGesture("Ctrl+Alt+S")
            .AddTo(fileMenu);

        return fileMenu;
    }
}