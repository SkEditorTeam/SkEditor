using System;
using System.ComponentModel;
using System.Linq;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.Utilities.Styling;

namespace SkEditor.Services.Settings;

public class SettingsObserverService(AppSettings appSettings, IThemeService themeService, ILogService logs)
{
    public void Initialize()
    {
        appSettings.PropertyChanged += OnAppSettingsChanged;
    }

    private async void OnAppSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        try
        {
            switch (e.PropertyName)
            {
                case nameof(AppSettings.CurrentTheme):
                {
                    string themeName = appSettings.CurrentTheme;
                    Theme? theme = themeService.Themes.FirstOrDefault(x => x.Name.Equals(themeName));

                    if (theme != null)
                    {
                        await themeService.SetThemeAsync(theme);
                    }

                    break;
                }
            }
        }
        catch (Exception ex)
        {
            logs.Error(ex, "An error occurred while applying the new settings.");
        }
    }
}