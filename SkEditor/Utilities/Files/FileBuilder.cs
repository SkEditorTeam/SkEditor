using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Completion;
using SkEditor.Utilities.Editor;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Styling;
using AssociationSelectionWindow = SkEditor.Views.Windows.FileTypes.AssociationSelectionWindow;
using GoToLineWindow = SkEditor.Views.Windows.GoToLineWindow;
using MainWindow = SkEditor.Views.Windows.MainWindow;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;

namespace SkEditor.Utilities.Files;

public class FileBuilder
{
    public static readonly Dictionary<string, FileTypes.FileType> OpenedFiles = [];

    public static async Task<TabViewItem?> Build(string header, string path = "", string? content = null)
    {
        FileTypes.FileType? fileType = await GetFileDisplay(path, content);
        if (fileType == null)
        {
            return null;
        }

        TabViewItem tabViewItem = new()
        {
            Header = header,
            IsSelected = true,
            Content = fileType.Display,
            Tag = string.Empty
        };

        MainWindow.Instance.BottomBar.IsVisible = fileType.NeedsBottomBar;

        if (!string.IsNullOrWhiteSpace(path))
        {
            tabViewItem.Tag = Uri.UnescapeDataString(path);

            ToolTip toolTip = new()
            {
                Content = path
            };

            ToolTip.SetShowDelay(tabViewItem, 1200);
            ToolTip.SetTip(tabViewItem, toolTip);
        }

        if (fileType is { IsEditor: true, Display: TextEditor editor })
        {
            SkEditorAPI.Events.FileCreated(editor);
            Dispatcher.UIThread.Post(() => TextEditorEventHandler.CheckForHex(editor));
        }

        OpenedFiles.Remove(header);
        OpenedFiles.Add(header, fileType);
        return tabViewItem;
    }

    private static async Task<FileTypes.FileType?> GetFileDisplay(string path, string? content)
    {
        FileTypes.FileType? fileType = null;
        if (!FileTypes.RegisteredFileTypes.ContainsKey(Path.GetExtension(path)))
        {
            return fileType ?? await GetDefaultEditor(path, content);
        }

        List<FileTypes.FileAssociation> handlers = FileTypes.RegisteredFileTypes[Path.GetExtension(path)];
        if (handlers.Count == 1)
        {
            fileType = handlers[0].Handle(path);
        }
        else
        {
            string ext = Path.GetExtension(path);
            if (SkEditorAPI.Configs.GetStateConfig().FileTypeChoices.TryGetValue(ext, out string? value))
            {
                if (value == "SkEditor")
                {
                    fileType = handlers.Find(association => !association.IsFromAddon)?.Handle(path);
                }
                else
                {
                    FileTypes.FileAssociation? preferred = handlers.Find(association => association.IsFromAddon &&
                        association.Addon?.Name == value);
                    if (preferred != null)
                    {
                        fileType = preferred.Handle(path);
                    }
                    else
                    {
                        SkEditorAPI.Configs.GetStateConfig().FileTypeChoices.Remove(ext);
                    }
                }
            }

            if (fileType != null)
            {
                return fileType;
            }

            AssociationSelectionWindow window = new(handlers);
            await window.ShowDialog(MainWindow.Instance);
            FileTypes.FileAssociation? selected = window.SelectedAssociation;
            if (selected == null)
            {
                return fileType ?? await GetDefaultEditor(path, content);
            }

            fileType = selected.Handle(path);
            if (window.RememberCheck.IsChecked == true)
            {
                SkEditorAPI.Configs.GetStateConfig().FileTypeChoices[ext] =
                    selected.IsFromAddon ? selected.Addon?.Name ?? "Addon" : "SkEditor";
            }
            else
            {
                SkEditorAPI.Configs.GetStateConfig().FileTypeChoices.Remove(ext);
            }

            _ = SkEditorAPI.Configs.GetStateConfig().Save();
        }

        return fileType ?? await GetDefaultEditor(path, content);
    }

    private static async Task<FileTypes.FileType?> GetDefaultEditor(string path, string? content)
    {
        string? fileContent = null;
        if (!string.IsNullOrWhiteSpace(path))
        {
            path = Uri.UnescapeDataString(path);
            if (File.Exists(path))
            {
                fileContent = content ?? await File.ReadAllTextAsync(path);
            }
        }

        if (fileContent != null && fileContent.Any(c => char.IsControl(c) && c != '\n' && c != '\r' && c != '\t'))
        {
            ContentDialogResult response = await SkEditorAPI.Windows.ShowDialog(
                Translation.Get(TranslationKey.BinaryFileTitle), Translation.Get(TranslationKey.BinaryFileFound),
                new SymbolIconSource { Symbol = Symbol.Alert });
            if (response != ContentDialogResult.Primary)
            {
                return null;
            }
        }

        TextEditor editor = CreateEditor();
        if (string.IsNullOrWhiteSpace(path))
        {
            return new FileTypes.FileType(editor, path, true);
        }

        path = Uri.UnescapeDataString(path);
        if (File.Exists(path))
        {
            editor.Text = fileContent;
        }

        return new FileTypes.FileType(editor, path, true);
    }

    public static TextEditor CreateEditor(string content = "")
    {
        Application? application = Application.Current;

        TextEditor editor = new()
        {
            ShowLineNumbers = true,
            Foreground = (ImmutableSolidColorBrush?)application?.FindResource("EditorTextColor"),
            Background = (ImmutableSolidColorBrush?)application?.FindResource("EditorBackgroundColor"),
            LineNumbersForeground = (ImmutableSolidColorBrush?)application?.FindResource("LineNumbersColor"),
            FontSize = 16,
            WordWrap = SkEditorAPI.Configs.GetAppSettings().IsWordWrapEnabled
        };

        editor.ContextFlyout = GetContextMenu(editor);

        string? fontFamily = SkEditorAPI.Configs.GetAppSettings().EditorFontFamily;
        if (fontFamily is null)
        {
            object? font = GetJetbrainsMonoFont();
            if (font != null)
            {
                editor.FontFamily = (FontFamily)font;
            }
            else
            {
                editor.FontFamily = new FontFamily("JetBrains Mono");
            }
        }
        else
        {
            editor.FontFamily = new FontFamily(fontFamily);
        }

        editor.Text = content;
        editor = AddEventHandlers(editor);
        editor = SetOptions(editor);

        return editor;
    }

    public static object? GetJetbrainsMonoFont()
    {
        object? font = null;
        Application.Current?.TryGetResource("JetBrainsFont", ThemeVariant.Default, out font);
        return font;
    }

    private static TextEditor AddEventHandlers(TextEditor editor)
    {
        editor.TextArea.PointerWheelChanged += TextEditorEventHandler.OnZoom;
        editor.TextArea.Loaded += (_, _) => editor.Focus();
        editor.TextChanged += TextEditorEventHandler.OnTextChanged;
        editor.TextArea.TextEntered += TextEditorEventHandler.DoAutoIndent;
        editor.TextArea.TextEntered += TextEditorEventHandler.DoAutoPairing;
        if (!SkEditorAPI.Configs.GetExperimentsConfig().RealtimeCodeParserEnabled)
        {
            editor.TextChanged += (_, _) => SkEditorAPI.Files.GetCurrentOpenedFile()?.Parser?.SetUnparsed();
        }

        if (SkEditorAPI.Configs.GetExperimentsConfig().HexPreviewEnabled)
        {
            editor.Document.TextChanged += (_, _) => TextEditorEventHandler.CheckForHex(editor);
        }

        editor.TextArea.Caret.PositionChanged += (_, _) =>
        {
            SkEditorAPI.Windows.GetMainWindow()?.BottomBar.UpdatePosition();
        };
        editor.TextArea.KeyDown += TextEditorEventHandler.OnKeyDown;
        editor.TextArea.TextView.PointerPressed += TextEditorEventHandler.OnPointerPressed;
        editor.TextArea.SelectionChanged += SelectionHandler.OnSelectionChanged;

        if (SkEditorAPI.Configs.GetExperimentsConfig().AutoCompletionPrototypeEnabled)
        {
            editor.TextChanged += CompletionHandler.OnTextChanged;
            editor.TextArea.AddHandler(InputElement.KeyDownEvent, CompletionHandler.OnKeyDown, handledEventsToo: true,
                routes: RoutingStrategies.Tunnel);
        }

        editor.TextArea.TextPasting += TextEditorEventHandler.OnTextPasting;

        return editor;
    }

    private static TextEditor SetOptions(TextEditor editor)
    {
        editor.TextArea.TextView.LinkTextForegroundBrush = new ImmutableSolidColorBrush(Color.Parse("#1a94c4"));
        editor.TextArea.TextView.LinkTextUnderline = true;
        editor.TextArea.SelectionBrush = (ImmutableSolidColorBrush?)Application.Current?.FindResource("SelectionColor");
        editor.TextArea.RightClickMovesCaret = true;

        LineNumberMargin? lineNumberMargin = editor.TextArea.LeftMargins.OfType<LineNumberMargin>().FirstOrDefault();

        lineNumberMargin?.Margin = new Thickness(10, 0);

        editor.Options.AllowScrollBelowDocument = true;
        editor.Options.CutCopyWholeLine = true;

        editor.Options.ConvertTabsToSpaces = !SkEditorAPI.Configs.GetAppSettings().UseEditorTabsForIndentation;
        editor.Options.IndentationSize = SkEditorAPI.Configs.GetAppSettings().EditorTabSize;

        editor.TextArea.TextView.CurrentLineBackground = ThemeEditor.CurrentTheme.CurrentLineBackground;
        editor.TextArea.TextView.CurrentLineBorder = new ImmutablePen(ThemeEditor.CurrentTheme.CurrentLineBorder, 2);
        editor.Options.HighlightCurrentLine = SkEditorAPI.Configs.GetAppSettings().HighlightCurrentLine;

        return editor;
    }

    public static MenuFlyout GetContextMenu(TextEditor editor)
    {
        MenuCommand[] commands =
        [
            new(TranslationKey.MenuHeaderCopy, new RelayCommand(editor.Copy), Symbol.Copy),
            new(TranslationKey.MenuHeaderPaste, new RelayCommand(editor.Paste), Symbol.Clipboard),
            new(TranslationKey.MenuHeaderCut, new RelayCommand(editor.Cut), Symbol.Cut),
            new(TranslationKey.MenuHeaderUndo, new RelayCommand(() => editor.Undo()), Symbol.ArrowUndo),
            new(TranslationKey.MenuHeaderRedo, new RelayCommand(() => editor.Redo()), Symbol.ArrowRedo),
            new(TranslationKey.MenuHeaderDuplicate,
                new RelayCommand(() => CustomCommandsHandler.OnDuplicateCommandExecuted(editor.TextArea)), Symbol.Copy),
            new(TranslationKey.MenuHeaderComment,
                new RelayCommand(() => CustomCommandsHandler.OnCommentCommandExecuted(editor.TextArea)),
                Symbol.Comment),
            new(TranslationKey.MenuHeaderGoToLine,
                new RelayCommand(() => SkEditorAPI.Windows.ShowWindow(new GoToLineWindow())),
                Symbol.TextNumberList),
            new(TranslationKey.MenuHeaderTrimWhitespaces,
                new RelayCommand(() => CustomCommandsHandler.OnTrimWhitespacesCommandExecuted(editor.TextArea)),
                Symbol.Eraser),
            new(TranslationKey.MenuHeaderDelete, new RelayCommand(editor.Delete), Symbol.Delete),
            new(TranslationKey.MenuHeaderRefactor,
                new AsyncRelayCommand(async () => await CustomCommandsHandler.OnRefactorCommandExecuted(editor)),
                Symbol.Rename)
        ];

        MenuFlyout contextMenu = new();

        foreach (MenuCommand item in commands)
        {
            contextMenu.Items.Add(new MenuItem
            {
                Header = Translation.Get(item.HeaderKey),
                Command = item.Command,
                Icon = new SymbolIcon { Symbol = item.Icon, FontSize = 20 }
            });
        }

        return contextMenu;
    }

    private record MenuCommand(TranslationKey HeaderKey, ICommand Command, Symbol Icon);
}