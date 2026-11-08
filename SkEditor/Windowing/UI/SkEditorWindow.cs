using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Appearance.Themes;

namespace SkEditor.Windowing.UI;

public class SkEditorWindow : FAAppWindow, IDisposable
{
    protected override Type StyleKeyOverride => typeof(FAAppWindow);
    
    private readonly IThemeService _themes = App.Services.GetRequiredService<IThemeService>();

    protected SkEditorWindow()
    {
        Focusable = true;

        KeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        };

        CanMaximize = false;
        CanMinimize = false;
        CanResize = false;
        ShowAsDialog = true;

        UpdateTheme();
        
        _themes.ThemeChanged += UpdateTheme;
    }

    private void UpdateTheme()
    {
        Background = Application.Current?.FindResource("SmallWindowBackgroundColor") as IBrush;

        if (_themes.CurrentTheme.UseMicaEffect)
        {
            TransparencyLevelHint = [WindowTransparencyLevel.Mica];
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _themes.ThemeChanged -= UpdateTheme;
    }
}