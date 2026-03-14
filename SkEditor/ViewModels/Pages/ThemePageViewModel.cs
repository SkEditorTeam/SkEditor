using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Styling;

namespace SkEditor.ViewModels.Pages;

public partial class ThemePageViewModel(IThemeService themeService) : ObservableObject
{
    public List<Theme> Themes { get; } = themeService.Themes;

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

            _ = Dispatcher.UIThread.InvokeAsync(() => themeService.SetTheme(value));
        }
    } = themeService.CurrentTheme;

    [RelayCommand]
    public void OpenThemesFolder()
    {
        Process.Start(new ProcessStartInfo { FileName = themeService.ThemeFolderPath, UseShellExecute = true });
    }
}