using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
            if (SkEditorAPI.Core.GetAppConfig().PreferredFileAssociations.TryGetValue(ext, out string? value))
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
                        SkEditorAPI.Core.GetAppConfig().PreferredFileAssociations.Remove(ext);
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
                SkEditorAPI.Core.GetAppConfig().PreferredFileAssociations[ext] =
                    selected.IsFromAddon ? selected.Addon?.Name ?? "Addon" : "SkEditor";
            }
            else
            {
                SkEditorAPI.Core.GetAppConfig().PreferredFileAssociations.Remove(ext);
            }

            SkEditorAPI.Core.GetAppConfig().Save();
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
                Translation.Get("BinaryFileTitle"), Translation.Get("BinaryFileFound"),
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
            WordWrap = SkEditorAPI.Core.GetAppConfig().IsWrappingEnabled
        };

        editor.ContextFlyout = GetContextMenu(editor);

        if (SkEditorAPI.Core.GetAppConfig().Font.Equals("Default"))
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
            editor.FontFamily = new FontFamily(SkEditorAPI.Core.GetAppConfig().Font);
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
        if (!SkEditorAPI.Core.GetAppConfig().EnableRealtimeCodeParser)
        {
            editor.TextChanged += (_, _) => SkEditorAPI.Files.GetCurrentOpenedFile()?.Parser?.SetUnparsed();
        }

        if (SkEditorAPI.Core.GetAppConfig().EnableHexPreview)
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

        if (SkEditorAPI.Core.GetAppConfig().EnableAutoCompletionExperiment)
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

        if (lineNumberMargin is not null)
        {
            lineNumberMargin.Margin = new Thickness(10, 0);
        }

        editor.Options.AllowScrollBelowDocument = true;
        editor.Options.CutCopyWholeLine = true;

        editor.Options.ConvertTabsToSpaces = SkEditorAPI.Core.GetAppConfig().UseSpacesInsteadOfTabs;
        editor.Options.IndentationSize = SkEditorAPI.Core.GetAppConfig().TabSize;

        editor.TextArea.TextView.CurrentLineBackground = ThemeEditor.CurrentTheme.CurrentLineBackground;
        editor.TextArea.TextView.CurrentLineBorder = new ImmutablePen(ThemeEditor.CurrentTheme.CurrentLineBorder, 2);
        editor.Options.HighlightCurrentLine = SkEditorAPI.Core.GetAppConfig().HighlightCurrentLine;

        return editor;
    }

    public static MenuFlyout GetContextMenu(TextEditor editor)
    {
        object[] commands =
        [
            new { Header = "MenuHeaderCopy", Command = new RelayCommand(editor.Copy), Icon = Symbol.Copy },
            new { Header = "MenuHeaderPaste", Command = new RelayCommand(editor.Paste), Icon = Symbol.Clipboard },
            new { Header = "MenuHeaderCut", Command = new RelayCommand(editor.Cut), Icon = Symbol.Cut },
            new { Header = "MenuHeaderUndo", Command = new RelayCommand(() => editor.Undo()), Icon = Symbol.ArrowUndo },
            new { Header = "MenuHeaderRedo", Command = new RelayCommand(() => editor.Redo()), Icon = Symbol.ArrowRedo },
            new
            {
                Header = "MenuHeaderDuplicate",
                Command = new RelayCommand(() => CustomCommandsHandler.OnDuplicateCommandExecuted(editor.TextArea)),
                Icon = Symbol.Copy
            },
            new
            {
                Header = "MenuHeaderComment",
                Command = new RelayCommand(() => CustomCommandsHandler.OnCommentCommandExecuted(editor.TextArea)),
                Icon = Symbol.Comment
            },
            new
            {
                Header = "MenuHeaderGoToLine",
                Command = new RelayCommand(() => SkEditorAPI.Windows.ShowWindow(new GoToLineWindow())),
                Icon = Symbol.TextNumberList
            },
            new
            {
                Header = "MenuHeaderTrimWhitespaces",
                Command =
                    new RelayCommand(() => CustomCommandsHandler.OnTrimWhitespacesCommandExecuted(editor.TextArea)),
                Icon = Symbol.Eraser
            },
            new { Header = "MenuHeaderDelete", Command = new RelayCommand(editor.Delete), Icon = Symbol.Delete },
            new
            {
                Header = "MenuHeaderRefactor",
                Command = new AsyncRelayCommand(async () =>
                    await CustomCommandsHandler.OnRefactorCommandExecuted(editor)),
                Icon = Symbol.Rename
            }
        ];

        MenuFlyout contextMenu = new();
        foreach (dynamic item in commands)
        {
            contextMenu.Items.Add(new MenuItem
            {
                Header = Translation.Get(item.Header),
                Command = item.Command,
                Icon = new SymbolIcon { Symbol = item.Icon, FontSize = 20 }
            });
        }

        return contextMenu;
    }
}