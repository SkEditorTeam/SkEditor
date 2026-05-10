using FluentIcons.Common;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Localization;
using MainMenuItem = SkEditor.ViewModels.Menu.MainMenuItem;

namespace SkEditor.Utilities.Menu.Providers;

public class EditMenuProvider(IWindowDialogService windowDialogs, ITabService tabs) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem edit = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderEdit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCopy, Symbol.Copy)
            .WithCommand(() => RequestAction(EditorCommandType.Copy))
            .WithInputGesture("Ctrl+C", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPaste, Symbol.ClipboardPaste)
            .WithCommand(() => RequestAction(EditorCommandType.Paste))
            .WithInputGesture("Ctrl+V", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCut, Symbol.Cut)
            .WithCommand(() => RequestAction(EditorCommandType.Cut))
            .WithInputGesture("Ctrl+X", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderUndo, Symbol.ArrowUndo)
            .WithCommand(() => RequestAction(EditorCommandType.Undo))
            .WithInputGesture("Ctrl+Z", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderRedo, Symbol.ArrowRedo)
            .WithCommand(() => RequestAction(EditorCommandType.Redo))
            .WithInputGesture("Ctrl+Y", false)
            .AddTo(edit);

        MainMenuItem.Separator.AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderDuplicate, Symbol.CopySelect)
            .WithCommand(() => RequestAction(EditorCommandType.Duplicate))
            .WithInputGesture("Ctrl+D", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderComment, Symbol.Comment)
            .WithCommand(() => RequestAction(EditorCommandType.Comment))
            .WithInputGesture("Ctrl+OemQuestion", false)
            .AddTo(edit);

        MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderTrimWhitespaces, Symbol.Eraser)
            .WithCommand(() => RequestAction(EditorCommandType.TrimWhitespace))
            .AddTo(edit);

        MainMenuItem.Separator.AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderGoToLine, Symbol.NumberSymbol)
            .WithCommand(windowDialogs.ShowGoToLineDialogAsync)
            .WithInputGesture("Ctrl+G")
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderFind, Symbol.Search)
            .WithCommand(() => RequestAction(EditorCommandType.Find))
            .WithInputGesture("Ctrl+F", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderReplace, Symbol.ArrowSwap)
            .WithCommand(() => RequestAction(EditorCommandType.Replace))
            .WithInputGesture("Ctrl+H", false)
            .AddTo(edit);

        return edit;
    }

    private void RequestAction(EditorCommandType commandType)
    {
        if (tabs.SelectedTab is not FileTab fileTab)
        {
            return;
        }

        fileTab.RequestEditorCommand(commandType);
    }
}