using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using Theme = SkEditor.Models.UI.Theme;

namespace SkEditor.ViewModels.Pages;

public partial class ThemePageViewModel(IThemeService themeService) : ObservableObject
{
    public Theme SelectedTheme
    {
        get;
        set
        {
            if (Equals(value, field))
            {
                return;
            }

            field = value;
            OnPropertyChanged();

            _ = Dispatcher.UIThread.InvokeAsync(() => themeService.SetThemeAsync(value));
        }
    } = themeService.CurrentTheme;

    public List<Theme> Themes { get; } = themeService.Themes;

    [RelayCommand]
    public void OpenThemesFolder()
    {
        Process.Start(new ProcessStartInfo { FileName = themeService.ThemeFolderPath, UseShellExecute = true });
    }
}