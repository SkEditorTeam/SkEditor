using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using AvaloniaEdit;
using FluentAvalonia.Styling;
using Newtonsoft.Json;
using SkEditor.API;
using SkEditor.Utilities.Files;

namespace SkEditor.Utilities.Styling;

public class ThemeEditor
{
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
        { "CurrentLineBorder", ["CurrentLineBorder"] }
    };

    public static List<Theme> Themes { get; set; } = [];
    public static Theme CurrentTheme { get; set; } = new();

    public static string ThemeFolderPath { get; set; } = Path.Combine(AppConfig.AppDataFolderPath, "Themes");

    public static Dictionary<string, ImmutableSolidColorBrush> DefaultColors { get; set; } = [];
    
    private static bool _defaultColorsCollected;

    public static ControlTheme SmallWindowTheme { get; private set; }

    public static void LoadThemes()
    {
        if (!Directory.Exists(ThemeFolderPath))
        {
            Directory.CreateDirectory(ThemeFolderPath);
        }

        if (!File.Exists(Path.Combine(ThemeFolderPath, "Default.json")))
        {
            SaveTheme(GetDefaultTheme());
        }

        string[] files = Directory.GetFiles(ThemeFolderPath);

        Themes.Clear();
        files.Where(x => Path.GetExtension(x) == ".json").ToList().ForEach(x => LoadTheme(x));

        Themes = [.. Themes.OrderBy(x => x.FileName.Equals("Default.json") ? 0 : 1)];

        CurrentTheme = GetUsedTheme();
    }

    public static Theme LoadTheme(string path)
    {
        Theme theme;
        try
        {
            string json = File.ReadAllText(path);
            
            theme = JsonConvert.DeserializeObject<Theme>(json);
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

    public static Theme GetDefaultTheme()
    {
        return new Theme
        {
            Name = "Default",
            FileName = "Default.json"
        };
    }

    public static void SaveTheme(Theme theme)
    {
        string path = Path.Combine(ThemeFolderPath, theme.FileName);

        string serializedTheme = JsonConvert.SerializeObject(theme, Formatting.Indented);
        File.WriteAllText(path, serializedTheme);
    }

    public static void SaveAllThemes()
    {
        Themes.ForEach(SaveTheme);
    }

    public static async Task SetTheme(Theme theme)
    {
        if (CurrentTheme.UseMicaEffect && !theme.UseMicaEffect)
        {
            SkEditorAPI.Windows.GetMainWindow().TransparencyLevelHint = [WindowTransparencyLevel.None];
        }
        
        CurrentTheme = theme;
        SkEditorAPI.Core.GetAppConfig().CurrentTheme = theme.FileName;

        await ApplyTheme();
    }

    public static async Task ApplyTheme()
    {
        if (_defaultColorsCollected)
        {
            CaptureNewDefaultColors(CurrentTheme);
        }
        else
        {
            CollectDefaultColors();
            _defaultColorsCollected = true;
        }

        RestoreDefaultColors();

        List<Task> tasks = [];

        foreach (KeyValuePair<string, string[]> item in ThemeToResourceDictionary)
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

    private static void ApplyMica()
    {
        if (!Application.Current.Resources.TryGetResource("SmallWindowTheme", ThemeVariant.Default,
                out object smallWindowTheme))
        {
            SkEditorAPI.Logs.Error("Failed to get SmallWindowTheme resource.", true);
            return;
        }

        ControlTheme smallWindow = (ControlTheme)smallWindowTheme;
        WindowTransparencyLevel[] levels =
            [WindowTransparencyLevel.Mica, WindowTransparencyLevel.AcrylicBlur, WindowTransparencyLevel.Blur];

        if (CurrentTheme.UseMicaEffect)
        {
            Setter? existingSetter = smallWindow.Setters.OfType<Setter>()
                .FirstOrDefault(x => x.Property.Name == "TransparencyLevelHint");
            if (existingSetter != null)
            {
                smallWindow.Setters.Remove(existingSetter);
            }
            smallWindow.Setters.Add(new Setter(TopLevel.TransparencyLevelHintProperty, levels));
            SkEditorAPI.Windows.GetMainWindow().TransparencyLevelHint = levels;
        }
        else
        {
            Setter? existingSetter = smallWindow.Setters.OfType<Setter>()
                .FirstOrDefault(x => x.Property.Name == "TransparencyLevelHint");
            if (existingSetter != null)
            {
                smallWindow.Setters.Remove(existingSetter);
            }

            SkEditorAPI.Windows.GetMainWindow().TransparencyLevelHint = [WindowTransparencyLevel.None];
        }

        SmallWindowTheme = smallWindow;
    }
    
    private static void CaptureNewDefaultColors(Theme themeToApply)
    {
        themeToApply.CustomColorChanges
            .Where(kvp => !DefaultColors.ContainsKey(kvp.Key))
            .ToList()
            .ForEach(kvp =>
            {
                if (Application.Current.TryGetResource(kvp.Key, ThemeVariant.Dark, out object? defaultColor))
                {
                    switch (defaultColor)
                    {
                        case SolidColorBrush brush:
                            DefaultColors.Add(kvp.Key, new ImmutableSolidColorBrush(brush));
                            break;
                        case ImmutableSolidColorBrush immutableBrush:
                            DefaultColors.Add(kvp.Key, immutableBrush);
                            break;
                    }
                }
                else
                {
                    SkEditorAPI.Logs.Warning($"Could not find default resource for key '{kvp.Key}' introduced by theme '{themeToApply.Name}'. It might not reset correctly.");
                }
            });
    }


    private static void CollectDefaultColors()
    {
        DefaultColors.Clear();
        
        foreach (string resourceKey in ThemeToResourceDictionary.SelectMany(item => item.Value))
        {
            if (DefaultColors.ContainsKey(resourceKey) ||
                !Application.Current.TryGetResource(resourceKey, ThemeVariant.Dark, out object? defaultColor))
            {
                continue;
            }

            switch (defaultColor)
            {
                case SolidColorBrush brush:
                    DefaultColors.Add(resourceKey, new ImmutableSolidColorBrush(brush));
                    break;
                case ImmutableSolidColorBrush immutableBrush:
                    DefaultColors.Add(resourceKey, immutableBrush);
                    break;
            }
        }
        
        foreach (var key in CurrentTheme.CustomColorChanges.Keys)
        {
            if (DefaultColors.ContainsKey(key) ||
                !Application.Current.TryGetResource(key, ThemeVariant.Dark, out object? defaultColor))
            {
                continue;
            }

            switch (defaultColor)
            {
                case SolidColorBrush brush:
                    DefaultColors.Add(key, new ImmutableSolidColorBrush(brush));
                    break;
                case ImmutableSolidColorBrush immutableBrush:
                    DefaultColors.Add(key, immutableBrush);
                    break;
            }
        }
        
        _defaultColorsCollected = true;
    }

    private static void RestoreDefaultColors()
    {
        foreach (var kvp in DefaultColors)
        {
            Application.Current.Resources[kvp.Key] = kvp.Value;
        }
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