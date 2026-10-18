using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media;

namespace SkEditor.Appearance.Themes;

/// <summary>
///     Manages the app themes, including loading, saving, and applying themes.
/// </summary>
public interface IThemeService
{
    /// <summary>
    ///     Gets the current theme of the application.
    /// </summary>
    Theme CurrentTheme { get; }

    /// <summary>
    ///     Gets or sets the path to the folder where themes are stored.
    /// </summary>
    string ThemeFolderPath { get; set; }

    /// <summary>
    ///     Gets or sets the list of available themes in the application.
    /// </summary>
    List<Theme> Themes { get; set; }

    /// <summary>
    ///     Loads all themes from the theme folder.
    /// </summary>
    void LoadThemes();

    /// <summary>
    ///     Loads a theme from the specified path.
    /// </summary>
    /// <param name="path">The path to the theme file.</param>
    /// <param name="isBuiltIn">Indicates whether the theme is built-in.</param>
    Theme? LoadTheme(string path, bool isBuiltIn = false);

    /// <summary>
    ///     Saves all themes to the theme folder.
    /// </summary>
    void SaveAllThemes();

    /// <summary>
    ///     Reloads the current theme asynchronously.
    /// </summary>
    Task ReloadCurrentThemeAsync();

    /// <summary>
    ///     Reloads all themes asynchronously.
    /// </summary>
    Task ReloadThemesAsync();

    /// <summary>
    ///     Sets the provided theme asynchronously.
    /// </summary>
    Task SetThemeAsync(Theme theme);

    /// <summary>
    ///     Applies the current theme asynchronously.
    /// </summary>
    /// <returns></returns>
    Task ApplyThemeAsync();

    /// <summary>
    ///     Gets the accent color of the current theme.
    /// </summary>
    Color GetAccentColor();

    /// <summary>
    ///     Fires when the current theme has changed.
    /// </summary>
    event Action ThemeChanged;

    /// <summary>
    ///     Fires when the themes have been reloaded.
    /// </summary>
    event Action ThemesReloaded;
}