using System.Collections.Generic;
using System.Threading.Tasks;
using Theme = SkEditor.Models.UI.Theme;

namespace SkEditor.Contracts.Services;

public interface IThemeService
{
    List<Theme> Themes { get; set; }
    Theme CurrentTheme { get; set; }
    string ThemeFolderPath { get; set; }
    void LoadThemes();
    Theme LoadThemeAsync(string path, bool isBuiltIn = false);
    void SaveAllThemes();
    Task ReloadCurrentThemeAsync();
    Task SetThemeAsync(Theme theme);
    Task ApplyThemeAsync();
}