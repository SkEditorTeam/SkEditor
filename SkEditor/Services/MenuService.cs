using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using FluentIcons.Common;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Menu;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Views.Windows;
using SkEditor.Views.Windows.Generators;
using SkEditor.Views.Windows.Generators.Gui;

namespace SkEditor.Services;

public class MenuService(ITabService tabService, IThemeService themeService, AppSettings appSettings) : IMenuService
{
    public IEnumerable<MainMenuItem> GetMainMenuItems()
    {
        MainMenuItem devToolsItem = BuildDevToolsMenu();
        devToolsItem.IsVisible = appSettings.IsDevModeEnabled;
        appSettings.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AppSettings.IsDevModeEnabled))
            {
                devToolsItem.IsVisible = appSettings.IsDevModeEnabled;
            }
        };

        List<MainMenuItem> items =
        [
            BuildFileMenu(),
            BuildEditMenu(),
            BuildToolsMenu(),
            BuildHelpMenu(),
            devToolsItem,
            BuildSettingsItem()
        ];

        return items;
    }

    private MainMenuItem BuildFileMenu()
    {
        MainMenuItem newFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderNew, Symbol.Document)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+N");

        MainMenuItem openFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpen, Symbol.DocumentArrowUp)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+O");

        MainMenuItem openFolder = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpenFolder, Symbol.Folder)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+Shift+O");

        MainMenuItem saveFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSave, Symbol.Save)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+S");

        MainMenuItem saveAsFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAs, Symbol.SaveEdit)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+Shift+S");

        MainMenuItem saveAllFiles = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAll, Symbol.SaveMultiple)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+Alt+S");

        MainMenuItem publishFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPublish, Symbol.Share)
            .WithCommand(OpenWindowDialog(() => App.Services.GetRequiredService<PublishWindow>()))
            .WithInputGesture("Ctrl+Shift+P");

        MainMenuItem closeMenu = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderClose, Symbol.Dismiss)
            .WithItems([
                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseCurrentFile, Symbol.Document)
                    .WithCommand(() => Task.CompletedTask)
                    .WithInputGesture("Ctrl+W"),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseAll, Symbol.DocumentMultiple)
                    .WithCommand(() => Task.CompletedTask)
                    .WithInputGesture("Ctrl+Shift+W"),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseAllButCurrent, Symbol.DocumentMultiple)
                    .WithCommand(() => Task.CompletedTask),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseUnsaved, Symbol.DocumentMultiple)
                    .WithCommand(() => Task.CompletedTask),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseLeft, Symbol.DocumentArrowLeft)
                    .WithCommand(() => Task.CompletedTask),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseRight, Symbol.DocumentArrowRight)
                    .WithCommand(() => Task.CompletedTask)
            ]);

        return MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderFile).WithItems([
            newFile,
            openFile,
            openFolder,
            MainMenuItem.Separator,
            saveFile,
            saveAsFile,
            saveAllFiles,
            MainMenuItem.Separator,
            publishFile,
            closeMenu
        ]);
    }

    private static MainMenuItem BuildEditMenu()
    {
        MainMenuItem copy = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCopy, Symbol.Copy)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+C", false);

        MainMenuItem paste = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPaste, Symbol.ClipboardPaste)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+V", false);

        MainMenuItem cut = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCut, Symbol.Cut)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+X", false);

        MainMenuItem undo = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderUndo, Symbol.ArrowUndo)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+Z", false);

        MainMenuItem redo = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderRedo, Symbol.ArrowRedo)
            .WithCommand(() => Task.CompletedTask)
            .WithInputGesture("Ctrl+Y", false);

        MainMenuItem duplicate = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderDuplicate, Symbol.CopySelect)
            .WithCommand(() =>
            {
                // TextArea? textArea = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea;
                // if (textArea is not null)
                // {
                //     CustomCommandsHandler.OnDuplicateCommandExecuted(textArea);
                // }
            })
            .WithInputGesture("Ctrl+D", false);

        MainMenuItem comment = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderComment, Symbol.Comment)
            .WithCommand(() =>
            {
                // TextArea? textArea = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea;
                // if (textArea is not null)
                // {
                //     CustomCommandsHandler.OnCommentCommandExecuted(textArea);
                // }
            })
            .WithInputGesture("Ctrl+OemQuestion", false);

        MainMenuItem trimWhitespaces = MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderTrimWhitespaces, Symbol.Eraser)
            .WithCommand(() =>
            {
                // TextArea? textArea = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea;
                // if (textArea is not null)
                // {
                //     CustomCommandsHandler.OnTrimWhitespacesCommandExecuted();
                // }
            });

        MainMenuItem goToLine = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderGoToLine, Symbol.NumberSymbol)
            .WithCommand(() => ShowDialogIfEditorIsOpen(new GoToLineWindow()))
            .WithInputGesture("Ctrl+G");

        MainMenuItem find = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderFind, Symbol.Search)
            .WithCommand(() =>
            {
                // SearchPanel? searchPanel = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel;
                // if (searchPanel is null)
                // {
                //     return;
                // }
                //
                // searchPanel.IsReplaceMode = false;
                // searchPanel.Close();
                // searchPanel.Open();
                // searchPanel.Reactivate();
            })
            .WithInputGesture("Ctrl+F");

        MainMenuItem replace = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderReplace, Symbol.ArrowSwap)
            .WithCommand(() =>
            {
                // SearchPanel? searchPanel = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel;
                // if (searchPanel is null)
                // {
                //     return;
                // }
                //
                // searchPanel.IsReplaceMode = true;
                // searchPanel.Close();
                // searchPanel.Open();
                // searchPanel.Reactivate();
            })
            .WithInputGesture("Ctrl+H");

        return MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderEdit).WithItems([
            copy,
            paste,
            cut,
            undo,
            redo,
            MainMenuItem.Separator,
            duplicate,
            comment,
            trimWhitespaces,
            MainMenuItem.Separator,
            goToLine,
            find,
            replace
        ]);
    }

    private static MainMenuItem BuildToolsMenu()
    {
        MainMenuItem generateGui = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderGenerateGUI, Symbol.Window)
            .WithCommand(() => ShowDialogIfEditorIsOpen(new GuiGenerator(), false))
            .WithInputGesture("Ctrl+Shift+G");

        MainMenuItem generateCommand = MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderGenerateCommand, Symbol.Keyboard)
            .WithCommand(() => ShowDialogIfEditorIsOpen(new CommandGenerator(), false));

        MainMenuItem generate = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderGenerate, Symbol.Wand)
            .WithItems([generateGui, generateCommand]);

        MainMenuItem refactor = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderRefactor, Symbol.Beaker)
            .WithCommand(() => ShowDialogIfEditorIsOpen(new RefactorWindow(), false));

        MainMenuItem docs = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderDocumentation, Symbol.Library)
            .WithCommand(AddDocsTab);

        MainMenuItem colorSelector = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderColorSelector, Symbol.Color)
            .WithCommand(() => new ColorSelectionWindow().ShowDialogOnMainWindow());

        MainMenuItem marketplace = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderMarketplace, Symbol.ShoppingBag)
            .WithCommand(() => { });

        return MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderTools).WithItems([
            generate,
            refactor,
            docs,
            colorSelector,
            marketplace
        ]);
    }

    private static MainMenuItem BuildHelpMenu()
    {
        MainMenuItem checkForUpdates =
            MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCheckForUpdates, Symbol.ArrowSync);
        MainMenuItem appWiki = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderAppWiki, Symbol.BookQuestionMark);
        MainMenuItem communitySupport =
            MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCommunitySupport, Symbol.ChatHelp);
        MainMenuItem about = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderAbout, Symbol.Info);

        return MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderHelp).WithItems([
            checkForUpdates,
            appWiki,
            communitySupport,
            about
        ]);
    }

    private MainMenuItem BuildDevToolsMenu()
    {
        MainMenuItem refreshSyntax = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderRefreshSyntax, Symbol.Color)
            .WithCommand(() => Task.CompletedTask);

        MainMenuItem refreshTheme = MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderRefreshTheme, Symbol.PaintBucket)
            .WithCommand(async () => await themeService.ReloadCurrentTheme());

        return MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderDevTools).WithItems([
            refreshSyntax,
            refreshTheme
        ]);
    }

    private static MainMenuItem BuildSettingsItem()
    {
        return MainMenuItem.IconOnly(Symbol.Settings)
            .WithCommand(() => { })
            .WithInputGesture("Ctrl+,");
    }

    private static AsyncRelayCommand OpenWindowDialog(Func<FAAppWindow> windowFactory)
    {
        return new AsyncRelayCommand(async () => await windowFactory().ShowDialogOnMainWindow());
    }

    private static async Task ShowDialogIfEditorIsOpen(FAAppWindow window, bool openAsDialog = true)
    {
        // if (SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor == null)
        // {
        //     return;
        // }

        if (openAsDialog)
        {
            await window.ShowDialogOnMainWindow();
        }
        else
        {
            window.ShowOnMainWindow();
        }
    }

    private static void AddDocsTab()
    {
        // SymbolIconSource icon = new()
        // {
        //     Symbol = Symbol.Book
        // };
        // SkEditorAPI.Files.AddCustomTab(Translation.GetObservable(TranslationKey.DocumentationWindowTitle),
        //     new DocumentationControl(),
        //     icon: icon);
    }
}