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
        MainMenuItem newFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderNew, Symbol.Document)
            .WithCommand(fileService.NewFile)
            .WithInputGesture("Ctrl+N");

        MainMenuItem saveFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSave, Symbol.Save)
            .WithCommand(fileService.SaveFile)
            .WithInputGesture("Ctrl+S");

        return MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderFile).WithItems([
            newFile,
            saveFile
        ]);
    }
}