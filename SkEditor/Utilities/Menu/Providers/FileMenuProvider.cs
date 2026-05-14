using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Messages;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

public class FileMenuProvider(
    IFileService files,
    ITabService tabs,
    ITabCloseService tabClose,
    IWindowDialogService windowDialogs) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem fileMenu = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderFile);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderNew, Symbol.Document)
            .WithCommand(files.NewFile)
            .WithInputGesture("Ctrl+N")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpen, Symbol.DocumentSearch)
            .WithCommand(files.OpenFileAsync)
            .WithInputGesture("Ctrl+O")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderOpenFolder, Symbol.FolderOpen)
            .WithCommand(() => WeakReferenceMessenger.Default.Send(new OpenExplorerFolderMessage()))
            .WithInputGesture("Ctrl+Shift+O")
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

        MainMenuItem.Separator.AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPublish, Symbol.Share)
            .WithCommand(windowDialogs.ShowPublishDialogAsync)
            .WithInputGesture("Ctrl+Shift+P")
            .AddTo(fileMenu);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderClose, Symbol.Dismiss)
            .WithItems([
                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseCurrentFile, Symbol.Document)
                    .WithCommand(() => tabs.SelectedTab?.CloseCommand.Execute(null))
                    .WithInputGesture("Ctrl+W"),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseAll, Symbol.DocumentMultiple)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.All))
                    .WithInputGesture("Ctrl+Shift+W"),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseAllButCurrent, Symbol.DocumentMultiple)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.AllButCurrent)),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseUnsaved, Symbol.DocumentMultiple)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.Unsaved)),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseLeft, Symbol.DocumentArrowLeft)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.Left)),

                MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCloseRight, Symbol.DocumentArrowRight)
                    .WithCommand(() => tabClose.CloseMultipleAsync(CloseType.Right))
            ])
            .AddTo(fileMenu);

        return fileMenu;
    }
}