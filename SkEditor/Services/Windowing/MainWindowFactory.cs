using System;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class MainWindowFactory(IServiceProvider sp, WindowRegistry registry)
{
    public MainWindow Create(SplashScreen? splashScreen = null)
    {
        var window = sp.GetRequiredService<MainWindow>();
        window.SetSplashScreen(splashScreen);
        registry.Register(window);
        return window;
    }
}