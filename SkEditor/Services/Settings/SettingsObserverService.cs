using System;
using System.ComponentModel;
using System.Linq;
using Avalonia.Threading;
using SkEditor.Contracts.Services;
using SkEditor.Utilities.Settings.Configs;
using Theme = SkEditor.Models.UI.Theme;

namespace SkEditor.Services.Settings;

// TODO: remove this and use R3 in Theme-related class
public class SettingsObserverService(AppSettings appSettings, IThemeService themeService, ILogService logs)
{
    public void Initialize()
    {
        appSettings.PropertyChanged += OnAppSettingsChanged;
    }

    private void OnAppSettingsChanged(object? sender, PropertyChangedEventArgs e)
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
                        _ = Dispatcher.UIThread.InvokeAsync(async () => { await themeService.SetThemeAsync(theme); });
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