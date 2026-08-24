using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;
using SkEditor.Models.UI;

namespace SkEditor.Contracts.Services;

public interface IThemeService
{
    Theme CurrentTheme { get; set; }
    string ThemeFolderPath { get; set; }
    List<Theme> Themes { get; set; }
    void LoadThemes();
    Theme? LoadTheme(string path, bool isBuiltIn = false);
    void SaveAllThemes();
    Task ReloadCurrentThemeAsync();
    Task ReloadThemesAsync();
    Task SetThemeAsync(Theme theme);
    Task ApplyThemeAsync();
    Color GetAccentColor();

    event Action ThemeChanged;
    event Action ThemesReloaded;
}