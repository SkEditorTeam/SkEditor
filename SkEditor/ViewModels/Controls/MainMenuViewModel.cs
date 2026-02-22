using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Input;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Search;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Windowing;
using FluentIcons.Avalonia.Fluent;
using FluentIcons.Common;
using SkEditor.API;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Projects;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using SkEditor.Views.Controls.Docs;
using SkEditor.Views.Windows;
using SkEditor.Views.Windows.Generators;
using SkEditor.Views.Windows.Generators.Gui;
using SkEditor.Views.Windows.Marketplace;
using SkEditor.Views.Windows.Settings;

namespace SkEditor.ViewModels.Controls;

public partial class MainMenuViewModel : ObservableObject
{
    [ObservableProperty]
    private IEnumerable<MainMenuItem> _items = new List<MainMenuItem>();

    public MainMenuViewModel()
    {
        AppSettings appSettings = SkEditorAPI.Configs.GetAppSettings();
        MainMenuItem devToolsItem = BuildDevToolsMenu();
        devToolsItem.IsVisible = appSettings.IsDevModeEnabled;
        appSettings.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(AppSettings.IsDevModeEnabled))
            {
                devToolsItem.IsVisible = appSettings.IsDevModeEnabled;
            }
        };

        Items = new List<MainMenuItem>
        {
            BuildFileMenu(),
            BuildEditMenu(),
            BuildToolsMenu(),
            BuildHelpMenu(),
            devToolsItem,
            BuildSettingsItem()
        };
    }

    private static MainMenuItem BuildFileMenu()
    {
        MainMenuItem newFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderNew, Symbol.Document)
            .WithCommand(FileHandler.NewFile)
            .WithInputGesture("Ctrl+N");

        MainMenuItem openFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpen, Symbol.DocumentArrowUp)
            .WithCommand(FileHandler.OpenFile)
            .WithInputGesture("Ctrl+O");

        MainMenuItem openFolder = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpenFolder, Symbol.Folder)
            .WithCommand(() => ProjectOpener.OpenProject())
            .WithInputGesture("Ctrl+Shift+O");

        MainMenuItem saveFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSave, Symbol.Save)
            .WithCommand(FileHandler.SaveFile)
            .WithInputGesture("Ctrl+S");

        MainMenuItem saveAsFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAs, Symbol.SaveEdit)
            .WithCommand(FileHandler.SaveAsFile)
            .WithInputGesture("Ctrl+Shift+S");

        MainMenuItem saveAllFiles = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderSaveAll, Symbol.SaveMultiple)
            .WithCommand(FileHandler.SaveAllFiles)
            .WithInputGesture("Ctrl+Alt+S");

        MainMenuItem publishFile = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPublish, Symbol.Share)
            .WithCommand(OpenWindowDialog(new PublishWindow()))
            .WithInputGesture("Ctrl+Shift+P");

        MainMenuItem closeMenu = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderClose, Symbol.Dismiss)
            .WithItems([
                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseCurrentFile, Symbol.Document)
                    .WithCommand(FileCloser.CloseCurrentFile)
                    .WithInputGesture("Ctrl+W"),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseAll, Symbol.DocumentMultiple)
                    .WithCommand(FileCloser.CloseAllFiles)
                    .WithInputGesture("Ctrl+Shift+W"),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseAllButCurrent, Symbol.DocumentMultiple)
                    .WithCommand(FileCloser.CloseAllExceptCurrent),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseUnsaved, Symbol.DocumentMultiple)
                    .WithCommand(FileCloser.CloseUnsaved),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseLeft, Symbol.DocumentArrowLeft)
                    .WithCommand(FileCloser.CloseAllToTheLeft),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseRight, Symbol.DocumentArrowRight)
                    .WithCommand(FileCloser.CloseAllToTheRight)
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
            .WithCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Copy())
            .WithInputGesture("Ctrl+C");

        MainMenuItem paste = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPaste, Symbol.ClipboardPaste)
            .WithCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Paste())
            .WithInputGesture("Ctrl+V");

        MainMenuItem cut = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCut, Symbol.Cut)
            .WithCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Cut())
            .WithInputGesture("Ctrl+X");

        MainMenuItem undo = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderUndo, Symbol.ArrowUndo)
            .WithCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Undo())
            .WithInputGesture("Ctrl+Z");

        MainMenuItem redo = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderRedo, Symbol.ArrowRedo)
            .WithCommand(() => SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.Redo())
            .WithInputGesture("Ctrl+Y");

        MainMenuItem duplicate = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderDuplicate, Symbol.CopySelect)
            .WithCommand(() =>
            {
                TextArea? textArea = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea;
                if (textArea is not null)
                {
                    CustomCommandsHandler.OnDuplicateCommandExecuted(textArea);
                }
            })
            .WithInputGesture("Ctrl+D");

        MainMenuItem comment = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderComment, Symbol.Comment)
            .WithCommand(() =>
            {
                TextArea? textArea = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea;
                if (textArea is not null)
                {
                    CustomCommandsHandler.OnCommentCommandExecuted(textArea);
                }
            })
            .WithInputGesture("Ctrl+OemQuestion");

        MainMenuItem trimWhitespaces = MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderTrimWhitespaces, Symbol.Eraser)
            .WithCommand(() =>
            {
                TextArea? textArea = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.TextArea;
                if (textArea is not null)
                {
                    CustomCommandsHandler.OnTrimWhitespacesCommandExecuted(textArea);
                }
            });

        MainMenuItem goToLine = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderGoToLine, Symbol.NumberSymbol)
            .WithCommand(() => ShowDialogIfEditorIsOpen(new GoToLineWindow()))
            .WithInputGesture("Ctrl+G");

        MainMenuItem find = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderFind, Symbol.Search)
            .WithCommand(() =>
            {
                SearchPanel? searchPanel = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel;
                if (searchPanel is null)
                {
                    return;
                }

                searchPanel.IsReplaceMode = false;
                searchPanel.Close();
                searchPanel.Open();
                searchPanel.Reactivate();
            })
            .WithInputGesture("Ctrl+F");

        MainMenuItem replace = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderReplace, Symbol.ArrowSwap)
            .WithCommand(() =>
            {
                SearchPanel? searchPanel = SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor?.SearchPanel;
                if (searchPanel is null)
                {
                    return;
                }

                searchPanel.IsReplaceMode = true;
                searchPanel.Close();
                searchPanel.Open();
                searchPanel.Reactivate();
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
            .WithCommand(() => new MarketplaceWindow().ShowDialogOnMainWindow());

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

    private static MainMenuItem BuildDevToolsMenu()
    {
        MainMenuItem refreshSyntax = MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderRefreshSyntax, Symbol.Color)
            .WithCommand(async () => await SyntaxLoader.RefreshSyntaxAsync());

        MainMenuItem refreshTheme = MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderRefreshTheme, Symbol.PaintBucket)
            .WithCommand(async () => await ThemeEditor.ReloadCurrentTheme());

        return MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderDevTools).WithItems([
            refreshSyntax,
            refreshTheme
        ]);
    }

    private static MainMenuItem BuildSettingsItem()
    {
        return MainMenuItem.IconOnly(Symbol.Settings)
            .WithCommand(() => new SettingsWindow().ShowDialogOnMainWindow())
            .WithInputGesture("Ctrl+,");
    }

    private static AsyncRelayCommand OpenWindowDialog(AppWindow window)
    {
        return new AsyncRelayCommand(async () => await window.ShowDialogOnMainWindow());
    }

    private static async Task ShowDialogIfEditorIsOpen(AppWindow window, bool openAsDialog = true)
    {
        if (SkEditorAPI.Files.GetCurrentOpenedFile()?.Editor == null)
        {
            return;
        }

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
        SymbolIconSource icon = new()
        {
            Symbol = Symbol.Book
        };
        SkEditorAPI.Files.AddCustomTab(Translation.GetObservable(TranslationKey.DocumentationWindowTitle),
            new DocumentationControl(),
            icon: icon);
    }

    public void RegisterHotKeys(MainWindow mainWindow)
    {
        RegisterHotKeysRecursive(Items);
        return;

        void RegisterHotKeysRecursive(IEnumerable<MainMenuItem> items)
        {
            foreach (MainMenuItem item in items)
            {
                if (!string.IsNullOrEmpty(item.InputGestureText) && item.Command is not null)
                {
                    KeyBinding keyBinding = new()
                    {
                        Command = item.Command,
                        Gesture = KeyGesture.Parse(item.InputGestureText)
                    };
                    mainWindow.KeyBindings.Add(keyBinding);
                }

                if (item.SubItems is not null)
                {
                    RegisterHotKeysRecursive(item.SubItems);
                }
            }
        }
    }
}

public partial class MainMenuItem : ObservableObject
{
    [ObservableProperty]
    private bool _isVisible = true;

    protected MainMenuItem()
    {
    }

    public TranslationKey? HeaderKey { get; init; }
    public Symbol? IconSymbol { get; init; }
    public IRelayCommand? Command { get; init; }
    public IEnumerable<MainMenuItem>? SubItems { get; init; }
    public string? InputGestureText { get; init; }
    public bool IsSeparator { get; init; }

    public static MainMenuItem Separator => new() { IsSeparator = true };

    private MainMenuItem CloneWith(
        TranslationKey? headerKey = null,
        Symbol? iconSymbol = null,
        IRelayCommand? command = null,
        IEnumerable<MainMenuItem>? subItems = null,
        string? inputGestureText = null,
        bool? isVisible = null)
    {
        return new MainMenuItem
        {
            HeaderKey = headerKey ?? HeaderKey,
            IconSymbol = iconSymbol ?? IconSymbol,
            Command = command ?? Command,
            SubItems = subItems ?? SubItems,
            InputGestureText = inputGestureText ?? InputGestureText,
            IsSeparator = IsSeparator,
            IsVisible = isVisible ?? IsVisible
        };
    }

    public static MainMenuItem HeaderOnly(TranslationKey headerKey)
    {
        return new MainMenuItem { HeaderKey = headerKey };
    }

    public static MainMenuItem IconOnly(Symbol iconSymbol)
    {
        return new MainMenuItem { IconSymbol = iconSymbol };
    }

    public static MainMenuItem HeaderWithIcon(TranslationKey headerKey, Symbol iconSymbol)
    {
        return new MainMenuItem
        {
            HeaderKey = headerKey,
            IconSymbol = iconSymbol
        };
    }

    public MainMenuItem WithItem(MainMenuItem subItem)
    {
        List<MainMenuItem> list = SubItems is not null ? [..SubItems] : [];
        list.Add(subItem);
        return CloneWith(subItems: list);
    }

    public MainMenuItem WithItems(IEnumerable<MainMenuItem> subItems)
    {
        List<MainMenuItem> list = SubItems is not null ? [..SubItems] : [];
        list.AddRange(subItems);
        return CloneWith(subItems: list);
    }

    public MainMenuItem WithIcon(Symbol iconSymbol)
    {
        return CloneWith(iconSymbol: iconSymbol);
    }

    public MainMenuItem WithCommand(IRelayCommand command)
    {
        return CloneWith(command: command);
    }

    public MainMenuItem WithCommand(Action execute)
    {
        return CloneWith(command: new RelayCommand(execute));
    }

    public MainMenuItem WithCommand(Func<Task> execute)
    {
        return CloneWith(command: new AsyncRelayCommand(execute));
    }

    public MainMenuItem WithInputGesture(string text)
    {
        return CloneWith(inputGestureText: text);
    }

    public MainMenuItem WithVisibility(bool isVisible)
    {
        return CloneWith(isVisible: isVisible);
    }
}