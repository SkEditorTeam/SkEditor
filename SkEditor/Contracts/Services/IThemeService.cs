using System.Collections.Generic;
using System.Threading.Tasks;
using SkEditor.Utilities.Styling;

namespace SkEditor.Contracts.Services;

public interface IThemeService
{
    List<Theme> Themes { get; set; }
    Theme CurrentTheme { get; set; }
    string ThemeFolderPath { get; set; }
    void LoadThemes();
    Theme LoadTheme(string path);
    void SaveAllThemes();
    Task ReloadCurrentTheme();
    Task SetTheme(Theme theme);
    Task ApplyTheme();
}