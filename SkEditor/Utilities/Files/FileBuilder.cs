using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Editing;
using CommunityToolkit.Mvvm.Input;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities.Completion;
using SkEditor.Utilities.Editor;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Files;

public class FileBuilder
{
    public async static Task<TabViewItem> Build(string header, string path = "")
    {
        TextEditor editor = await GetDefaultEditor(path);
        TabViewItem tabViewItem = new()
        {
            Header = header,
            IsSelected = true,
            Content = editor,
            Tag = string.Empty
        };

        if (!string.IsNullOrWhiteSpace(path))
        {
            tabViewItem.Tag = Uri.UnescapeDataString(path);

            ToolTip toolTip = new()
            {
                Content = path,
            };

            ToolTip.SetShowDelay(tabViewItem, 1200);
            ToolTip.SetTip(tabViewItem, toolTip);

            Icon.SetIcon(tabViewItem);
        }

        ApiVault.Get().OnFileCreated(editor);
        Dispatcher.UIThread.Post(() => TextEditorEventHandler.CheckForHex(editor));

        return tabViewItem;
    }

    private async static Task<TextEditor> GetDefaultEditor(string path)
    {
        AppConfig config = ApiVault.Get().GetAppConfig();

        TextEditor editor = new()
        {
            ShowLineNumbers = true,
            Foreground = (ImmutableSolidColorBrush)Application.Current.FindResource("EditorTextColor"),
            Background = (ImmutableSolidColorBrush)Application.Current.FindResource("EditorBackgroundColor"),
            LineNumbersForeground = (ImmutableSolidColorBrush)Application.Current.FindResource("LineNumbersColor"),
            FontSize = 16,
            WordWrap = config.IsWrappingEnabled,
        };

        editor.ContextFlyout = GetContextMenu(editor);

        if (config.Font.Equals("Default"))
        {
            Application.Current.TryGetResource("JetBrainsFont", Avalonia.Styling.ThemeVariant.Default, out object font);
            editor.FontFamily = (FontFamily)font;
        }
        else
        {
            editor.FontFamily = new FontFamily(config.Font);
        }

        if (!string.IsNullOrWhiteSpace(path))
        {
            path = Uri.UnescapeDataString(path);
            if (File.Exists(path))
            {
                editor.Text = await File.ReadAllTextAsync(path);
            }
        }

        editor = AddEventHandlers(editor);
        editor = SetOptions(editor);

        return editor;
    }

    private static TextEditor AddEventHandlers(TextEditor editor)
    {
        editor.TextArea.PointerWheelChanged += TextEditorEventHandler.OnZoom;
        editor.TextArea.Loaded += (sender, e) => editor.Focus();
        editor.TextChanged += TextEditorEventHandler.OnTextChanged;
        editor.TextArea.TextEntered += TextEditorEventHandler.DoAutoIndent;
        editor.TextArea.TextEntered += TextEditorEventHandler.DoAutoPairing;
        if (ApiVault.Get().GetAppConfig().EnableHexPreview)
        {
            editor.Document.TextChanged += (_, _) => TextEditorEventHandler.CheckForHex(editor);
        }
        editor.TextArea.Caret.PositionChanged += (sender, e) =>
        {
            ApiVault.Get().GetMainWindow().BottomBar.UpdatePosition();
        };
        editor.TextArea.KeyDown += TextEditorEventHandler.OnKeyDown;
        editor.TextArea.TextView.PointerPressed += TextEditorEventHandler.OnPointerPressed;
        editor.TextArea.SelectionChanged += SelectionHandler.OnSelectionChanged;

        if (ApiVault.Get().GetAppConfig().EnableAutoCompletionExperiment)
        {
            editor.TextChanged += CompletionHandler.OnTextChanged;
            editor.TextArea.AddHandler(Avalonia.Input.InputElement.KeyDownEvent, CompletionHandler.OnKeyDown, handledEventsToo: true, routes: RoutingStrategies.Tunnel);
        }

        editor.TextArea.TextPasting += TextEditorEventHandler.OnTextPasting;

        return editor;
    }

    private static TextEditor SetOptions(TextEditor editor)
    {
        editor.TextArea.TextView.LinkTextForegroundBrush = new ImmutableSolidColorBrush(Color.Parse("#1a94c4"));
        editor.TextArea.TextView.LinkTextUnderline = true;
        editor.TextArea.SelectionBrush = (ImmutableSolidColorBrush)Application.Current.FindResource("SelectionColor");

        editor.TextArea.LeftMargins.OfType<LineNumberMargin>().FirstOrDefault().Margin = new Thickness(10, 0);

        editor.Options.AllowScrollBelowDocument = true;
        editor.Options.CutCopyWholeLine = true;
        
        editor.Options.ConvertTabsToSpaces = ApiVault.Get().GetAppConfig().UseSpacesInsteadOfTabs;
        editor.Options.IndentationSize = ApiVault.Get().GetAppConfig().TabSize;

        return editor;
    }

    private static MenuFlyout GetContextMenu(TextEditor editor)
    {
        var commands = new[]
        {
            new { Header = "MenuHeaderCopy", Command = new RelayCommand(editor.Copy), Icon = Symbol.Copy },
            new { Header = "MenuHeaderPaste", Command = new RelayCommand(editor.Paste), Icon = Symbol.Paste },
            new { Header = "MenuHeaderCut", Command = new RelayCommand(editor.Cut), Icon = Symbol.Cut },
            new { Header = "MenuHeaderUndo", Command = new RelayCommand(() => editor.Undo()), Icon = Symbol.Undo },
            new { Header = "MenuHeaderRedo", Command = new RelayCommand(() => editor.Redo()), Icon = Symbol.Redo },
            new { Header = "MenuHeaderDuplicate", Command = new RelayCommand(() => CustomCommandsHandler.OnDuplicateCommandExecuted(editor.TextArea)), Icon = Symbol.Copy },
            new { Header = "MenuHeaderComment", Command = new RelayCommand(() => CustomCommandsHandler.OnCommentCommandExecuted(editor.TextArea)), Icon = Symbol.Comment },
            new { Header = "MenuHeaderDelete", Command = new RelayCommand(editor.Delete), Icon = Symbol.Delete }
        };

        var contextMenu = new MenuFlyout();
        commands.Select(item => new MenuItem
        {
            Header = Translation.Get(item.Header),
            Command = item.Command,
            Icon = new SymbolIcon { Symbol = item.Icon, FontSize = 20 }
        }).ToList().ForEach(item => contextMenu.Items.Add(item));

        return contextMenu;
    }
}