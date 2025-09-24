using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using AvaloniaEdit;
using FluentAvalonia.UI.Controls;
using SkEditor.API;
using SkEditor.Utilities;
using SkEditor.Utilities.Docs;
using SkEditor.Utilities.Docs.Local;
using SkEditor.Utilities.Docs.SkUnity;
using SkEditor.Utilities.Files;
using SkEditor.Utilities.Styling;
using SkEditor.Utilities.Syntax;
using MainWindow = SkEditor.Views.Windows.MainWindow;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;

namespace SkEditor.Views.Controls.Docs;

public partial class DocElementControl : UserControl
{
    private readonly DocumentationControl _documentationControl;
    private readonly IDocumentationEntry _entry;
    private bool _hasLoadedExamples;

    public DocElementControl(IDocumentationEntry entry, DocumentationControl documentationControl)
    {
        InitializeComponent();

        _documentationControl = documentationControl;
        _entry = entry;

        _ = LoadVisuals(entry);
        LoadPatternsEditor(entry);
        _ = SetupExamples(entry);
        _ = LoadDownloadButton();

        LoadExpressionChangers(entry);

        if (entry.DocType == IDocumentationEntry.Type.Event)
        {
            OtherElementPanel.Children.Add(CreateExpander(Translation.Get("DocumentationControlEventValues"),
                Format(string.IsNullOrEmpty(entry.EventValues)
                    ? Translation.Get("DocumentationControlNoEventValues")
                    : entry.EventValues)));
        }
    }

    private void LoadExpressionChangers(IDocumentationEntry entry)
    {
        if (entry.DocType != IDocumentationEntry.Type.Expression || string.IsNullOrEmpty(entry.Changers))
        {
            return;
        }


        Expander expander = CreateExpander(Translation.Get("DocumentationControlChangers"),
            Format(string.IsNullOrEmpty(entry.Changers)
                ? Translation.Get("DocumentationControlNoChangers")
                : entry.Changers));

        StackPanel buttons = new()
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Left,
            Margin = new Thickness(2),
            Spacing = 2
        };
        expander.Content = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 3,
            Children =
            {
                new TextBlock { Text = Translation.Get("DocumentationControlChangerHelp") },
                buttons
            }
        };

        foreach (string raw in entry.Changers.Split("\n"))
        {
            if (!Enum.TryParse(typeof(IDocumentationEntry.Changer), raw, true, out object? change))
            {
                buttons.Children.Add(new Button
                {
                    Content = raw,
                    IsEnabled = false
                });
                continue;
            }

            IDocumentationEntry.Changer changer = (IDocumentationEntry.Changer)change;
            Button button = new() { Content = raw };
            button.Click += async (_, _) =>
            {
                string firstPattern = GenerateUsablePattern(PatternsEditor.Text.Split("\n")[0]);
                const string value = "<value>";
                string format = changer switch
                {
                    IDocumentationEntry.Changer.Set => $"set %s to {value}",
                    IDocumentationEntry.Changer.Add => $"add {value} to %s",
                    IDocumentationEntry.Changer.Remove => $"remove {value} from %s",
                    IDocumentationEntry.Changer.Reset => "reset %s",
                    IDocumentationEntry.Changer.Clear => "clear %s",
                    IDocumentationEntry.Changer.Delete => "delete %s",
                    IDocumentationEntry.Changer.RemoveAll => "remove all %s",
                    _ => throw new NotImplementedException("Changer not implemented")
                };

                IClipboard? clipboard = MainWindow.Instance?.Clipboard;
                if (clipboard == null)
                {
                    return;
                }

                await clipboard.SetTextAsync(format.Replace("%s", firstPattern));
            };

            buttons.Children.Add(button);
        }

        OtherElementPanel.Children.Add(expander);
    }

    private async Task SetupExamples(IDocumentationEntry entry)
    {
        if (!IDocProvider.Providers[entry.Provider].NeedsToLoadExamples)
        {
            await LoadExamples(entry);
            return;
        }

        _hasLoadedExamples = false;
        ExamplesEntry.IsExpanded = false;

        ExamplesEntry.Expanded += async (_, _) =>
        {
            if (_hasLoadedExamples)
            {
                return;
            }

            _hasLoadedExamples = true;

            await LoadExamples(entry);
        };
    }

    private void LoadPatternsEditor(IDocumentationEntry entry)
    {
        PatternsEditor.TextArea.SelectionBrush = ThemeEditor.CurrentTheme.SelectionColor;
        if (SkEditorAPI.Core.GetAppConfig().Font.Equals("Default"))
        {
            object? font = FileBuilder.GetJetbrainsMonoFont();
            if (font == null)
            {
                return;
            }

            PatternsEditor.FontFamily = (FontFamily)font;
        }
        else
        {
            PatternsEditor.FontFamily = new FontFamily(SkEditorAPI.Core.GetAppConfig().Font);
        }

        PatternsEditor.Text = Format(entry.Patterns);
        PatternsEditor.SyntaxHighlighting = DocSyntaxColorizer.CreatePatternHighlighting();
        PatternsEditor.TextArea.TextView.Redraw();
    }

    protected async Task LoadVisuals(IDocumentationEntry entry)
    {
        NameText.Text = entry.Name;
        Expander.Description = entry.DocType + " from " + entry.Addon;
        Expander.Description = Translation.Get("DocumentationControlTypeFromProvider", entry.DocType.ToString(), entry.Addon);
        Expander.IconSource = IDocumentationEntry.GetTypeIcon(entry.DocType);
        DescriptionText.Text = Format(string.IsNullOrEmpty(entry.Description)
            ? Translation.Get("DocumentationControlNoDescription")
            : entry.Description);
        VersionBadge.IconSource = new FontIconSource
        {
            Glyph = Translation.Get("DocumentationControlSince",
                string.IsNullOrEmpty(entry.Version) ? "1.0.0" : entry.Version)
        };

        string? uri = IDocProvider.Providers[entry.Provider].GetLink(entry);
        OutsideButton.Content = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 2,
            Children =
            {
                new TextBlock
                {
                    Text = Translation.Get("DocumentationControlSeeOn", entry.Provider.ToString()),
                    VerticalAlignment = VerticalAlignment.Center
                },
                new SymbolIcon
                {
                    Symbol = Symbol.Open,
                    FontSize = 18,
                    VerticalAlignment = VerticalAlignment.Center
                }
            }
        };

        if (uri == null)
        {
            OutsideButton.IsVisible = false;
        }
        else
        {
            OutsideButton.Click += (_, _) => Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
        }

        await LoadAddonBadge(entry);
    }

    private async Task LoadAddonBadge(IDocumentationEntry entry)
    {
        SourceBadge.IconSource = new FontIconSource { Glyph = entry.Addon };

        Color? color = await IDocProvider.Providers[entry.Provider].GetAddonColor(entry.Addon);
        if (color == null)
        {
            return;
        }

        SourceBadge.Background = new SolidColorBrush(color.Value);
        SourceBadge.Foreground = color.Value.ToHsl().L < 0.2 ? Brushes.White : Brushes.Black;

        if (entry.Provider == DocProvider.skUnity)
        {
            SkUnityProvider? skUnityProvider = IDocProvider.Providers[DocProvider.skUnity] as SkUnityProvider;
            if (skUnityProvider == null)
            {
                return;
            }

            SourceBadge.Tapped += (_, _) =>
            {
                string uri = SkUnityProvider.GetAddonLink(entry.Addon);
                Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
            };
        }
    }

    public static Expander CreateExpander(string name, string content)
    {
        TextEditor editor = new()
        {
            Margin = new Thickness(5),
            FontSize = 16,
            Foreground = (IBrush?)GetResource("EditorTextColor"),
            Background = (IBrush?)GetResource("EditorBackgroundColor"),
            Padding = new Thickness(10),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            IsReadOnly = true,
            Text = content,
            TextArea =
            {
                SelectionBrush = ThemeEditor.CurrentTheme.SelectionColor
            }
        };

        if (SkEditorAPI.Core.GetAppConfig().Font.Equals("Default"))
        {
            object? font = FileBuilder.GetJetbrainsMonoFont();
            if (font is not null)
            {
                editor.FontFamily = (FontFamily)font;
            }
        }
        else
        {
            editor.FontFamily = new FontFamily(SkEditorAPI.Core.GetAppConfig().Font);
        }

        return new Expander
        {
            Header = name,
            Content = editor
        };
    }

    private static object? GetResource(string key)
    {
        object? resource = null;
        Application.Current?.TryGetResource(key, ThemeVariant.Default, out resource);
        return resource;
    }

    public void DeleteElementFromCache(bool removeFromParent = false)
    {
        LocalProvider? localProvider = LocalProvider.Get();
        if (localProvider == null) return;
        
        _ = localProvider.RemoveElement(_entry);
        if (removeFromParent)
        {
            _documentationControl.RemoveElement(this);
        }
    }

    public async Task DownloadElementToCache()
    {
        List<IDocumentationExample> examples;
        try
        {
            examples = await IDocProvider.Providers[_entry.Provider].FetchExamples(_entry);
        }
        catch (Exception e)
        {
            SkEditorAPI.Logs.Error($"Failed to fetch examples for {_entry.Name}: {e.Message}");
            examples = [];
            await SkEditorAPI.Windows.ShowError(Translation.Get("DocumentationControlErrorExamples", e.Message));
        }

        LocalProvider? localProvider = LocalProvider.Get();
        if (localProvider == null) return;
        await localProvider.DownloadElement(_entry, examples);
    }

    public async void DownloadButtonClicked(object? sender, RoutedEventArgs args)
    {
        try
        {
            if (_entry.Provider == DocProvider.Local)
            {
                DeleteElementFromCache(true);
                EnableDownloadButton();
            }
            else
            {
                LocalProvider? localProvider = LocalProvider.Get();
                if (localProvider != null && await localProvider.IsElementDownloaded(_entry))
                {
                    DeleteElementFromCache();
                    EnableDownloadButton();
                }
                else
                {
                    await DownloadElementToCache();
                    DisableDownloadButton();
                }
            }
        }
        catch (Exception e)
        {
            await SkEditorAPI.Windows.ShowError($"An error occurred while downloading the element: {e.Message}");
            EnableDownloadButton();
        }
    }

    public void EnableDownloadButton()
    {
        DownloadElementButton.Content = new TextBlock { Text = Translation.Get("DocumentationControlDownload") };
        DownloadElementButton.Classes.Add("accent");
    }

    public void DisableDownloadButton()
    {
        DownloadElementButton.Content = new TextBlock { Text = Translation.Get("DocumentationControlRemove") };
        DownloadElementButton.Classes.Remove("accent");
    }

    public async Task ForceDownloadElement()
    {
        LocalProvider? localProvider = LocalProvider.Get();
        if (localProvider != null && await localProvider.IsElementDownloaded(_entry))
        {
            return;
        }

        await DownloadElementToCache();
        DisableDownloadButton();
    }

    public async Task LoadDownloadButton()
    {
        LocalProvider? localProvider = LocalProvider.Get();
        DownloadElementButton.Click += DownloadButtonClicked;
        DownloadElementButton.Classes.Clear();

        if (_entry.Provider == DocProvider.Local || localProvider != null && await localProvider.IsElementDownloaded(_entry))
        {
            DisableDownloadButton();
        }
        else
        {
            EnableDownloadButton();
        }
    }

    public async Task LoadExamples(IDocumentationEntry entry)
    {
        IDocProvider provider = IDocProvider.Providers[entry.Provider];

        // First we setup a small loading bar
        ExamplesEntry.Content = new ProgressBar
        {
            IsIndeterminate = true,
            Height = 5,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Margin = new Thickness(5)
        };

        // Then we load the examples
        try
        {
            List<IDocumentationExample> examples = await provider.FetchExamples(entry);
            ExamplesEntry.Content = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Margin = new Thickness(5)
            };

            if (examples.Count == 0)
            {
                ExamplesEntry.Content = new TextBlock
                {
                    Text = Translation.Get("DocumentationControlNoExamples"),
                    Foreground = Brushes.Gray,
                    TextWrapping = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontStyle = FontStyle.Italic
                };
            }

            foreach (IDocumentationExample example in examples)
            {
                StackPanel stackPanel = new()
                {
                    Orientation = Orientation.Vertical,
                    Margin = new Thickness(0, 2)
                };

                stackPanel.Children.Add(new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Spacing = 5,
                    Children =
                    {
                        new TextBlock
                        {
                            Text = Translation.Get("DocumentationControlExampleAuthor", example.Author),
                            FontWeight = FontWeight.Regular,
                            FontSize = 16,
                            Margin = new Thickness(0, 0, 0, 5)
                        },
                        new InfoBadge
                        {
                            IconSource = new FontIconSource { Glyph = example.Votes },
                            VerticalAlignment = VerticalAlignment.Top
                        }
                    }
                });

                TextEditor textEditor = new()
                {
                    Foreground = (IBrush?)GetResource("EditorTextColor"),
                    Background = (IBrush?)GetResource("EditorBackgroundColor"),
                    Padding = new Thickness(10),
                    Text = Format(example.Example),
                    IsReadOnly = true,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(0, 0, 0, 10),
                    TextArea =
                    {
                        SelectionBrush = ThemeEditor.CurrentTheme.SelectionColor
                    }
                };

                if (SkEditorAPI.Core.GetAppConfig().Font.Equals("Default"))
                {
                    object? font = FileBuilder.GetJetbrainsMonoFont();
                    if (font is not null)
                    {
                        textEditor.FontFamily = (FontFamily)font;
                    }
                }
                else
                {
                    textEditor.FontFamily = new FontFamily(SkEditorAPI.Core.GetAppConfig().Font);
                }

                textEditor.SyntaxHighlighting = SyntaxLoader.GetCurrentSkriptHighlighting();
                stackPanel.Children.Add(textEditor);

                ((StackPanel)ExamplesEntry.Content).Children.Add(stackPanel);
            }
        }
        catch (Exception e)
        {
            ExamplesEntry.Content = new TextBlock
            {
                Text = Translation.Get("DocumentationControlErrorExamples", e.Message),
                Foreground = Brushes.Red,
                FontWeight = FontWeight.SemiLight,
                TextWrapping = TextWrapping.Wrap
            };
        }
    }

    private static string Format(string input)
    {
        return input.Replace("&gt;", ">").Replace("&lt;", "<").Replace("&amp;", "&")
            .Replace("&quot;", "\"").Replace("&apos;", "'")
            .Replace("&#039;", "'").Replace("&#034;", "\"");
    }

    private static string GenerateUsablePattern(string pattern)
    {
        while (BracketedTextRegex().IsMatch(pattern))
        {
            pattern = BracketedTextRegex().Replace(pattern, "");
        }

        // Step 2: Select the first option within ()
        pattern = FirstOptionInParenthesesRegex().Replace(pattern, "$1");

        // Step 3: Leave everything within %% untouched (already handled by not modifying it)

        // Trim any extra whitespace
        pattern = WhitespaceRegex().Replace(pattern, " ").Trim();

        return pattern;
    }

    [GeneratedRegex(@"\(([^|]+)\|.*?\)")]
    private static partial Regex FirstOptionInParenthesesRegex();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();

    [GeneratedRegex(@"\[([^[\]])*?\]")]
    private static partial Regex BracketedTextRegex();

    #region Actions

    private void FilterByThisType(object? sender, RoutedEventArgs e)
    {
        _documentationControl.FilterByType(_entry.DocType);
    }

    private void FilterByThisAddon(object? sender, RoutedEventArgs e)
    {
        _documentationControl.FilterByAddon(_entry.Addon);
    }

    #endregion
}