using FluentIcons.Common;
using R3;
using SkEditor.Core.Localization;
using SkEditor.Editor.Commands;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Layout.Menu.Models;
using SkEditor.Windowing;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Extensions;

namespace SkEditor.Layout.Menu.Providers;

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
            .WithInputGesture("Ctrl+D")
            .AddTo(edit);

        MainMenuItem.HeaderWithIcon(TranslationKey.MenuHeaderComment, Symbol.Comment)
            .WithCommand(() => RequestAction(EditorCommandType.Comment))
            .WithInputGesture("Ctrl+OemQuestion")
            .AddTo(edit);

        MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderTrimWhitespaces, Symbol.Eraser)
            .WithCommand(() => RequestAction(EditorCommandType.TrimWhitespace))
            .AddTo(edit);
        
        MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderIndentation, Symbol.TextIndentIncrease)
            .WithItems(
                MainMenuItem
                    .HeaderWithIcon(TranslationKey.MenuHeaderIndentationSpacesToTabs, Symbol.KeyboardTab)
                    .WithCommand(() => RequestAction(EditorCommandType.SpacesToTabs)),
                MainMenuItem
                    .HeaderWithIcon(TranslationKey.MenuHeaderIndentationTabsTospaces, Symbol.Spacebar)
                    .WithCommand(() => RequestAction(EditorCommandType.TabsToSpaces))
            )
            .AddTo(edit);

        MainMenuItem
            .HeaderWithIcon(TranslationKey.MenuHeaderTransform, Symbol.TextCaseTitle)
            .WithItems(
                MainMenuItem
                    .HeaderWithIcon(TranslationKey.MenuHeaderTransformUppercase, Symbol.TextCaseUppercase)
                    .WithTooltip(TranslationKey.MenuTooltipTransformUppercase)
                    .WithCommand(() => RequestAction(EditorCommandType.TransformToUppercase)),
                MainMenuItem
                    .HeaderWithIcon(TranslationKey.MenuHeaderTransformLowercase, Symbol.TextCaseLowercase)
                    .WithTooltip(TranslationKey.MenuTooltipTransformLowercase)
                    .WithCommand(() => RequestAction(EditorCommandType.TransformToLowercase)),
                MainMenuItem
                    .HeaderWithIcon(TranslationKey.MenuHeaderTransformTitlecase, Symbol.TextCaseTitle)
                    .WithTooltip(TranslationKey.MenuTooltipTransformTitlecase)
                    .WithCommand(() => RequestAction(EditorCommandType.TransformToTitlecase)),
                MainMenuItem
                    .HeaderWithIcon(TranslationKey.MenuHeaderTransformSmallCaps, Symbol.FontDecrease)
                    .WithTooltip(TranslationKey.MenuTooltipTransformSmallCaps)
                    .WithCommand(() => RequestAction(EditorCommandType.TransformToSmallCaps))
            )
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

        BindableReactiveProperty<bool> isTextEditorActive = tabs.ObserveSelectedTab()
            .Select(tab => tab is ITextEditorTab)
            .ToBindableReactiveProperty();

        foreach (MainMenuItem item in edit.SubItems ?? [])
        {
            item.IsEnabled = isTextEditorActive;
        }

        return edit;
    }

    private void RequestAction(EditorCommandType commandType)
    {
        if (tabs.SelectedTab is not FileTabViewModel fileTab)
        {
            return;
        }

        fileTab.RequestEditorCommand(commandType);
    }
}