using FluentIcons.Common;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Menu;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Menu.Providers;

public class EditMenuProvider(IWindowDialogService windowDialogs, ITabService tabs) : IMenuProvider
{
    public MainMenuItem GetMainMenuItem()
    {
        MainMenuItem edit = MainMenuItem.HeaderOnly(TranslationKey.MenuHeaderEdit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCopy, Symbol.Copy)
            .WithCommand(() => RequestAction(EditorActionType.Copy))
            .WithInputGesture("Ctrl+C", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderPaste, Symbol.ClipboardPaste)
            .WithCommand(() => RequestAction(EditorActionType.Paste))
            .WithInputGesture("Ctrl+V", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderCut, Symbol.Cut)
            .WithCommand(() => RequestAction(EditorActionType.Cut))
            .WithInputGesture("Ctrl+X", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderUndo, Symbol.ArrowUndo)
            .WithCommand(() => RequestAction(EditorActionType.Undo))
            .WithInputGesture("Ctrl+Z", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderRedo, Symbol.ArrowRedo)
            .WithCommand(() => RequestAction(EditorActionType.Redo))
            .WithInputGesture("Ctrl+Y", false)
            .AddTo(edit);

        MainMenuItem.Separator.AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderDuplicate, Symbol.CopySelect)
            .WithCommand(() => RequestAction(EditorActionType.Duplicate))
            .WithInputGesture("Ctrl+D", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderComment, Symbol.Comment)
            .WithCommand(() => RequestAction(EditorActionType.Comment))
            .WithInputGesture("Ctrl+OemQuestion", false)
            .AddTo(edit);

        MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderTrimWhitespaces, Symbol.Eraser)
            .WithCommand(() => RequestAction(EditorActionType.TrimWhitespace))
            .AddTo(edit);

        MainMenuItem.Separator.AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderGoToLine, Symbol.NumberSymbol)
            .WithCommand(windowDialogs.ShowGoToLineDialogAsync)
            .WithInputGesture("Ctrl+G")
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderFind, Symbol.Search)
            .WithCommand(() => RequestAction(EditorActionType.Find))
            .WithInputGesture("Ctrl+F", false)
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderReplace, Symbol.ArrowSwap)
            .WithCommand(() => RequestAction(EditorActionType.Replace))
            .WithInputGesture("Ctrl+H", false)
            .AddTo(edit);

        return edit;
    }

    private void RequestAction(EditorActionType actionType)
    {
        if (tabs.SelectedTab is not FileTab fileTab)
        {
            return;
        }

        fileTab.RequestEditorAction(actionType);
    }
}