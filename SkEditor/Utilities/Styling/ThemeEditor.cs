using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using AvaloniaEdit;
using FluentAvalonia.Styling;
using Newtonsoft.Json;
using SkEditor.API;
using SkEditor.Utilities.Files;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Styling;

public class ThemeEditor
{
    public static List<Theme> Themes { get; set; } = [];
    public static Theme CurrentTheme { get; set; } = new();

    public static string ThemeFolderPath { get; set; } = Path.Combine(AppConfig.AppDataFolderPath, "Themes");

    public static readonly Dictionary<string, string[]> ThemeToResourceDictionary = new()
    {
        { "BackgroundColor", ["BackgroundColor"] },
        {
            "SmallWindowBackgroundColor",
            ["SmallWindowBackgroundColor", "ContentDialogBackground", "TaskDialogButtonAreaBackground"]
        },
        { "EditorBackgroundColor", ["EditorBackgroundColor"] },
        { "EditorTextColor", ["EditorTextColor"] },
        { "LineNumbersColor", ["LineNumbersColor"] },
        { "SelectionColor", ["SelectionColor"] },
        { "SelectedTabItemBackground", ["TabViewItemHeaderBackgroundSelected"] },
        { "SelectedTabItemBorder", ["TabViewSelectedItemBorderBrush"] },
        {
            "MenuBackground",
            ["MenuFlyoutPresenterBackground", "ComboBoxDropDownBackground", "FlyoutPresenterBackground"]
        },
        {
            "MenuBorder",
            ["MenuFlyoutPresenterBorderBrush", "ComboBoxDropDownBorderBrush", "FlyoutBorderThemeBrush"]
        },
        { "TextBoxFocusedBackground", ["TextControlBackgroundFocused"] },
        { "CurrentLineBackground", ["CurrentLineBackground"] },
        { "CurrentLineBorder", ["CurrentLineBorder"] },
    };

    public static Dictionary<string, ImmutableSolidColorBrush> DefaultColors { get; set; } = [];

    public static void LoadThemes()
    {
        if (!Directory.Exists(ThemeFolderPath))
        {
            Directory.CreateDirectory(ThemeFolderPath);
        }

        if (!File.Exists(Path.Combine(ThemeFolderPath, "Default.json"))) SaveTheme(GetDefaultTheme());

        string[] files = Directory.GetFiles(ThemeFolderPath);

        files.Where(x => Path.GetExtension(x) == ".json").ToList().ForEach(x => LoadTheme(x));

        Themes = [.. Themes.OrderBy(x => x.FileName.Equals("Default.json") ? 0 : 1)];

        CurrentTheme = GetUsedTheme();
    }

    public static Theme LoadTheme(string path)
    {
        Theme theme;
        try
        {
            theme = JsonConvert.DeserializeObject<Theme>(File.ReadAllText(path));
            if (theme == null)
            {
                File.Delete(path);
                theme = GetDefaultTheme();
            }

            theme.FileName = Path.GetFileName(path);
            Themes.Add(theme);
        }
        catch
        {
            theme = GetDefaultTheme();
        }

        return theme;
    }

    private static Theme GetUsedTheme()
    {
        string currentTheme = SkEditorAPI.Core.GetAppConfig().CurrentTheme;

        if (currentTheme != null && File.Exists(Path.Combine(ThemeFolderPath, currentTheme)))
        {
            Theme selectedTheme = Themes.FirstOrDefault(x => x.FileName.Equals(currentTheme));
            return selectedTheme ?? GetDefaultTheme();
        }

        if (File.Exists(Path.Combine(ThemeFolderPath, "Default.json")))
        {
            return Themes.FirstOrDefault(x => x.FileName.Equals("Default.json")) ?? GetDefaultTheme();
        }

        return GetDefaultTheme();
    }

    public static Theme GetDefaultTheme() => new()
    {
        Name = "Default",
        FileName = "Default.json"
    };

    public static void SaveTheme(Theme theme)
    {
        string path = Path.Combine(ThemeFolderPath, theme.FileName);

        string serializedTheme = JsonConvert.SerializeObject(theme, Formatting.Indented);
        File.WriteAllText(path, serializedTheme);
    }

    public static void SaveAllThemes() => Themes.ForEach(SaveTheme);

    public static async Task SetTheme(Theme theme)
    {
        CurrentTheme = theme;
        SkEditorAPI.Core.GetAppConfig().CurrentTheme = theme.FileName;

        await ApplyTheme();
    }

    public static async Task ApplyTheme()
    {
        SaveDefaultColors();

        var tasks = new List<Task>();

        foreach (var item in ThemeToResourceDictionary)
        {
            ImmutableSolidColorBrush brush = CurrentTheme.GetBrushByName(item.Key);

            tasks.AddRange(item.Value.Select(resource =>
            {
                Application.Current.Resources[resource] = brush;
                return Task.CompletedTask;
            }));
        }

        UpdateTextEditorColors();

        tasks.AddRange(CurrentTheme.CustomColorChanges.Select(colorChange =>
        {
            Application.Current.Resources[colorChange.Key] = colorChange.Value;
            return Task.CompletedTask;
        }));

        FluentAvaloniaTheme styles = Application.Current.Styles.OfType<FluentAvaloniaTheme>().First();
        styles.CustomAccentColor = CurrentTheme.AccentColor.Color;
        styles.PreferUserAccentColor = true;

        ApplyMica();
        UpdateFont();

        await Task.WhenAll(tasks);
    }

    public static ControlTheme SmallWindowTheme { get; private set; }

    private static void ApplyMica()
    {
        Uri uri = new("avares://SkEditor/Styles/OnlyCloseButtonWindow.axaml");
        var style = new ResourceInclude(uri) { Source = uri };

        if (!style.TryGetResource("SmallWindowTheme", ThemeVariant.Default, out var smallWindowTheme)) return;

        WindowTransparencyLevel[] levels =
            [WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur];

        ControlTheme smallWindow = (ControlTheme)smallWindowTheme;
        if (CurrentTheme.UseMicaEffect)
        {
            smallWindow.Setters.Add(new Setter(TopLevel.TransparencyLevelHintProperty, levels));
            SkEditorAPI.Windows.GetMainWindow().TransparencyLevelHint = levels;
        }
        else
        {
            smallWindow.Setters.Remove(smallWindow.Setters.OfType<Setter>()
                .FirstOrDefault(x => x.Property.Name == "TransparencyLevelHint"));
            SkEditorAPI.Windows.GetMainWindow().TransparencyLevelHint = [WindowTransparencyLevel.None];
        }

        Application.Current.Resources.MergedDictionaries.Add(style);
        SmallWindowTheme = smallWindow;
    }

    private static void SaveDefaultColors()
    {
        CurrentTheme.CustomColorChanges
            .Where(colorChange => !DefaultColors.ContainsKey(colorChange.Key))
            .ToList()
            .ForEach(colorChange =>
            {
                if (!Application.Current.TryGetResource(colorChange.Key, ThemeVariant.Dark, out var defaultColor))
                    return;
                if (defaultColor is SolidColorBrush brush)
                {
                    DefaultColors.Add(colorChange.Key, new ImmutableSolidColorBrush(brush));
                }
                else if (defaultColor.GetType() == typeof(ImmutableSolidColorBrush))
                {
                    DefaultColors.Add(colorChange.Key, (ImmutableSolidColorBrush)defaultColor);
                }
            });
    }

    private static void UpdateTextEditorColors()
    {
        List<OpenedFile> files = SkEditorAPI.Files.GetOpenedEditors();
        foreach (TextEditor textEditor in files.Select(x => x.Editor))
        {
            textEditor.Background = CurrentTheme.EditorBackgroundColor;
            textEditor.Foreground = CurrentTheme.EditorTextColor;
            textEditor.LineNumbersForeground = CurrentTheme.LineNumbersColor;
            textEditor.TextArea.TextView.CurrentLineBackground = CurrentTheme.CurrentLineBackground;
            textEditor.TextArea.TextView.CurrentLineBorder = new ImmutablePen(CurrentTheme.CurrentLineBorder, 2);
            textEditor.TextArea.TextView.InvalidateVisual();
        }
    }

    private static void UpdateFont()
    {
        // if (OSVersionHelper.IsWindows())
        // {
        //     fontName = OSVersionHelper.IsWindows11() ? "Segoe UI Variable Text" : "Segoe UI";
        // }
        // else
        // {
        //     fontName = FontFamily.Default.Name;
        // }
        string fontName = CurrentTheme.CustomFont ?? FontFamily.Default.Name;

        Application.Current.Resources["ContentControlThemeFontFamily"] = new FontFamily(fontName);
    }

    public static async Task ReloadCurrentTheme()
    {
        string themePath = Path.Combine(ThemeFolderPath, CurrentTheme.FileName);

        Themes.RemoveAll(t => t.FileName == CurrentTheme.FileName);

        Theme reloadedTheme = LoadTheme(themePath);
        CurrentTheme = reloadedTheme;

        await ApplyTheme();
    }
}