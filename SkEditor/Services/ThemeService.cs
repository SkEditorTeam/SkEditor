using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Styling;
using FluentAvalonia.Styling;
using SkEditor.Contracts.Services;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Settings.Configs;
using MainWindow = SkEditor.Views.Windows.MainWindow;
using Theme = SkEditor.Models.UI.Theme;
using ThemeJsonContext = SkEditor.Models.UI.ThemeJsonContext;

namespace SkEditor.Services;

public class ThemeService(
    ILogService log,
    IConfigService config,
    AppSettings appSettings,
    WindowRegistry windowRegistry) : IThemeService
{
    private bool _defaultColorsCollected;

    public Dictionary<string, ImmutableSolidColorBrush> DefaultColors { get; set; } = [];

    public List<Theme> Themes { get; set; } = [];
    public Theme CurrentTheme { get; set; } = new();

    public string ThemeFolderPath { get; set; } = Path.Combine(config.GetAppDataPath(), "Themes");

    public void LoadThemes()
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
        files.Where(x => Path.GetExtension(x) == ".json").ToList().ForEach(x => LoadThemeAsync(x));

        Themes = [.. Themes.OrderBy(x => x.FileName.Equals("Default.json") ? 0 : 1)];

        CurrentTheme = GetUsedTheme();
    }

    public Theme LoadThemeAsync(string path)
    {
        Theme? theme;
        try
        {
            string json = File.ReadAllText(path);
            theme = JsonSerializer.Deserialize<Theme>(json, ThemeJsonContext.Default.Theme);

            if (theme == null)
            {
                File.Delete(path);
                theme = GetDefaultTheme();
            }

            theme.FileName = Path.GetFileName(path);
            Themes.Add(theme);
        }
        catch (Exception ex)
        {
            log.Error($"Failed to load theme from {path}: {ex.Message}");
            theme = GetDefaultTheme();
        }

        return theme;
    }

    public void SaveAllThemes()
    {
        Themes.ForEach(SaveTheme);
    }

    public async Task SetThemeAsync(Theme theme)
    {
        if (CurrentTheme.UseMicaEffect && !theme.UseMicaEffect)
        {
            MainWindow? mainWindow = windowRegistry.Active;
            mainWindow?.TransparencyLevelHint = [WindowTransparencyLevel.None];
        }

        CurrentTheme = theme;
        appSettings.CurrentTheme = theme.Name;

        await ApplyThemeAsync();
    }

    public async Task ApplyThemeAsync()
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

        Application? currentApp = Application.Current;
        if (currentApp == null)
        {
            log.Error("Application.Current is null. Cannot apply theme.", true);
            return;
        }

        foreach (KeyValuePair<string, string[]> item in Theme.ThemeToResourceDictionary)
        {
            ImmutableSolidColorBrush brush = CurrentTheme.GetBrushByName(item.Key);

            tasks.AddRange(item.Value.Select(resource =>
            {
                currentApp.Resources[resource] = brush;
                return Task.CompletedTask;
            }));
        }

        UpdateTextEditorColors();

        tasks.AddRange(CurrentTheme.CustomColorChanges.Select(colorChange =>
        {
            currentApp.Resources[colorChange.Key] = colorChange.Value;
            return Task.CompletedTask;
        }));

        FluentAvaloniaTheme styles = currentApp.Styles.OfType<FluentAvaloniaTheme>().First();
        styles.CustomAccentColor = CurrentTheme.AccentColor.Color;
        styles.PreferUserAccentColor = true;

        UpdateFont();

        await Task.WhenAll(tasks);
    }

    public async Task ReloadCurrentThemeAsync()
    {
        string themePath = Path.Combine(ThemeFolderPath, CurrentTheme.FileName);

        Themes.RemoveAll(t => t.FileName == CurrentTheme.FileName);

        Theme reloadedTheme = LoadThemeAsync(themePath);
        CurrentTheme = reloadedTheme;

        await ApplyThemeAsync();
    }

    private Theme GetUsedTheme()
    {
        string currentTheme = appSettings.CurrentTheme;

        if (currentTheme != null)
        {
            Theme? selectedTheme = Themes.FirstOrDefault(x =>
                x.Name.Equals(currentTheme) || x.FileName.Equals(currentTheme));

            if (selectedTheme == null)
            {
                return GetDefaultTheme();
            }

            if (!selectedTheme.Name.Equals(currentTheme))
            {
                appSettings.CurrentTheme = selectedTheme.Name;
            }

            return selectedTheme;
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

    public void SaveTheme(Theme theme)
    {
        string path = Path.Combine(ThemeFolderPath, theme.FileName);

        string serializedTheme = JsonSerializer.Serialize(theme, ThemeJsonContext.Default.Theme);
        File.WriteAllText(path, serializedTheme);
    }

    private void CaptureNewDefaultColors(Theme themeToApply)
    {
        themeToApply.CustomColorChanges
            .Where(kvp => !DefaultColors.ContainsKey(kvp.Key))
            .ToList()
            .ForEach(kvp =>
            {
                if (Application.Current?.TryGetResource(kvp.Key, ThemeVariant.Dark, out object? defaultColor) == true)
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
                    log.Warning(
                        $"Could not find default resource for key '{kvp.Key}' introduced by theme '{themeToApply.Name}'. It might not reset correctly.");
                }
            });
    }


    private void CollectDefaultColors()
    {
        DefaultColors.Clear();

        foreach (string resourceKey in Theme.ThemeToResourceDictionary.SelectMany(item => item.Value))
        {
            object? defaultColor = null;
            if (DefaultColors.ContainsKey(resourceKey) ||
                Application.Current?.TryGetResource(resourceKey, ThemeVariant.Dark, out defaultColor) == false)
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

        foreach (string key in CurrentTheme.CustomColorChanges.Keys)
        {
            object? defaultColor = null;
            if (DefaultColors.ContainsKey(key) ||
                Application.Current?.TryGetResource(key, ThemeVariant.Dark, out defaultColor) == false)
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

    private void RestoreDefaultColors()
    {
        foreach (KeyValuePair<string, ImmutableSolidColorBrush> kvp in DefaultColors)
        {
            if (Application.Current != null)
            {
                Application.Current.Resources[kvp.Key] = kvp.Value;
            }
        }
    }

    private void UpdateTextEditorColors()
    {
        // List<OpenedFile> files = SkEditorAPI.Files.GetOpenedEditors();
        // foreach (TextEditor textEditor in files.Select(x => x.Editor).OfType<TextEditor>())
        // {
        //     textEditor.Background = CurrentTheme.EditorBackgroundColor;
        //     textEditor.Foreground = CurrentTheme.EditorTextColor;
        //     textEditor.LineNumbersForeground = CurrentTheme.LineNumbersColor;
        //     textEditor.TextArea.TextView.CurrentLineBackground = CurrentTheme.CurrentLineBackground;
        //     textEditor.TextArea.TextView.CurrentLineBorder = new ImmutablePen(CurrentTheme.CurrentLineBorder, 2);
        //     textEditor.TextArea.TextView.InvalidateVisual();
        // }
    }

    private void UpdateFont()
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

        if (Application.Current != null)
        {
            Application.Current.Resources["ContentControlThemeFontFamily"] = new FontFamily(fontName);
        }
    }
}