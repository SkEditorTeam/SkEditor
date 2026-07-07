using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAvalonia.Styling;
using Iciclecreek.Avalonia.Terminal;
using SkEditor.Contracts.Services;
using SkEditor.Models.UI;
using SkEditor.Services.Windowing;
using SkEditor.Utilities.Settings.Configs;
using MainWindow = SkEditor.Views.Windows.MainWindow;

namespace SkEditor.Services;

public class ThemeService(
    ILogService log,
    IConfigService config,
    AppSettings appSettings,
    IconProvider iconProvider,
    WindowRegistry windowRegistry) : IThemeService
{
    private bool _defaultColorsCollected;

    public Dictionary<string, ImmutableSolidColorBrush> DefaultColors { get; set; } = [];

    public List<Theme> Themes { get; set; } = [];
    public Theme CurrentTheme { get; set; } = new();

    public string ThemeFolderPath { get; set; } = Path.Combine(config.GetAppDataPath(), "Themes");

    public event Action? ThemeChanged;

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

        IEnumerable<Theme> loadedThemes = files
            .Where(x => Path.GetExtension(x).Equals(".json", StringComparison.OrdinalIgnoreCase))
            .Select(themeFile => LoadTheme(themeFile))
            .OfType<Theme>();

        foreach (Theme theme in loadedThemes)
        {
            Themes.Add(theme);
        }

        Themes = [.. Themes.OrderBy(x => x.FileName.Equals("Default.json") ? 0 : 1)];

        LoadBuiltInThemes();

        CurrentTheme = GetUsedTheme();
    }

    private void LoadBuiltInThemes()
    {
        Uri themesUri = new("avares://SkEditor/Assets/Themes/");
        IEnumerable<Uri> themes = AssetLoader.GetAssets(themesUri, themesUri);

        foreach (Uri theme in themes)
        {
            Theme? deserializedTheme = LoadTheme(theme.ToString(), true);

            if (deserializedTheme == null)
            {
                continue;
            }

            Themes.Add(deserializedTheme);
        }
    }

    public Theme? LoadTheme(string path, bool isBuiltIn = false)
    {
        Theme? theme;
        try
        {
            string json;

            if (isBuiltIn)
            {
                using Stream stream = AssetLoader.Open(new Uri(path));
                using StreamReader reader = new(stream);
                json = reader.ReadToEnd();
            }
            else
            {
                json = File.ReadAllText(path);
            }

            theme = JsonSerializer.Deserialize<Theme>(json, ThemeJsonContext.Default.Theme);

            if (theme == null)
            {
                return null;
            }

            if (theme.UseMicaEffect && !OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
            {
                return null;
            }

            if (isBuiltIn)
            {
                theme.IsBuiltIn = true;
                theme.FileName = Path.GetFileName(new Uri(path).LocalPath);
            }
            else
            {
                theme.FileName = Path.GetFileName(path);
            }
        }
        catch (Exception ex)
        {
            log.Error($"Failed to load theme from {path}: {ex.Message}");
            return null;
        }

        return theme;
    }

    public void SaveAllThemes()
    {
        Themes.ForEach(SaveTheme);
    }

    public async Task SetThemeAsync(Theme theme)
    {
        bool oldMica = CurrentTheme.UseMicaEffect;
        bool newMica = theme.UseMicaEffect;

        if (oldMica && !newMica)
        {
            MainWindow? mainWindow = windowRegistry.Active;
            mainWindow?.TransparencyLevelHint = [WindowTransparencyLevel.None];
        }

        if (newMica)
        {
            MainWindow? mainWindow = windowRegistry.Active;
            mainWindow?.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
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
        ApplyTerminalTheme();
        await UpdateExplorerIcons();

        tasks.AddRange(CurrentTheme.CustomColorChanges.Select(colorChange =>
        {
            currentApp.Resources[colorChange.Key] = colorChange.Value;
            return Task.CompletedTask;
        }));

        UpdateAccentColor(currentApp);
        currentApp.PlatformSettings?.ColorValuesChanged += (_, _) => { UpdateAccentColor(currentApp); };

        UpdateFont();

        ThemeChanged?.Invoke();

        await Task.WhenAll(tasks);
    }

    private async Task UpdateExplorerIcons()
    {
        await iconProvider.LoadIcons(CurrentTheme.ExplorerIconColors);
    }

    private void ApplyTerminalTheme()
    {
        ConsolePalette? palette = ConsolePalette.CreateDefault();

        foreach ((string key, string hex) in CurrentTheme.TerminalColors)
        {
            if (key.Length < 3)
            {
                continue;
            }

            bool isBg = key.StartsWith("Bg", StringComparison.OrdinalIgnoreCase);
            string colorName = key[2..];

            if (Enum.TryParse(colorName, true, out AnsiColor ansiColor) &&
                Color.TryParse(hex, out Color color))
            {
                palette[ansiColor, isBg] = color;
            }
        }

        BufferCellExtensions.ActivePalette = palette;
    }

    private void UpdateAccentColor(Application app)
    {
        FluentAvaloniaTheme styles = app.Styles.OfType<FluentAvaloniaTheme>().First();

        Color accent = GetAccentColor();

        styles.PreferUserAccentColor = false;
        styles.CustomAccentColor = accent;
    }

    public Color GetAccentColor()
    {
        if (CurrentTheme.AccentColor is not null)
        {
            return CurrentTheme.AccentColor.Color;
        }

        Color rawSystemColor = Application.Current?.PlatformSettings?.GetColorValues().AccentColor1 ?? Colors.White;

        HsvColor hsv = rawSystemColor.ToHsv();

        double targetS = Math.Clamp(hsv.S * 0.65, 0.40, 0.55);
        double targetV = Math.Clamp(hsv.V, 0.80, 0.90);

        HsvColor newHsv = new(rawSystemColor.A, hsv.H, targetS, targetV);

        return newHsv.ToRgb();
    }

    public async Task ReloadThemesAsync()
    {
        LoadThemes();
        await ReloadCurrentThemeAsync();
    }

    public async Task ReloadCurrentThemeAsync()
    {
        string themePath = CurrentTheme.IsBuiltIn
            ? $"avares://SkEditor/Assets/Themes/{CurrentTheme.FileName}"
            : Path.Combine(ThemeFolderPath, CurrentTheme.FileName);

        Theme? existingTheme = Themes.FirstOrDefault(t => t.FileName == CurrentTheme.FileName);
        int themeIndex = Themes.Count - 1;

        if (existingTheme != null)
        {
            themeIndex = Themes.IndexOf(existingTheme);
            Themes.Remove(existingTheme);
        }

        Theme? reloadedTheme = LoadTheme(themePath, CurrentTheme.IsBuiltIn);

        if (reloadedTheme == null)
        {
            log.Error($"Failed to reload theme from {themePath}. Keeping the current theme.");
            return;
        }

        Themes.Insert(themeIndex, reloadedTheme);
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
        if (Application.Current is null)
        {
            return;
        }

        string? customFont = CurrentTheme.CustomFont;

        object? font = null;
        if (customFont is not null)
        {
            font = new FontFamily(customFont);
        }
        else if (!OperatingSystem.IsWindows())
        {
            font = new FontFamily("avares://SkEditor/Assets/Fonts/OpenSans#Open Sans");
        }

        font ??= new FontFamily(FontFamily.Default.Name);
        Application.Current.Resources["ContentControlThemeFontFamily"] = font;
    }
}