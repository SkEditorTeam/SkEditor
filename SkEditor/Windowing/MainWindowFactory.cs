using System;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Windowing.UI;

namespace SkEditor.Windowing;

public class MainWindowFactory(IServiceProvider sp, IWindowRegistry registry)
{
    public MainWindow Create(SplashScreen? splashScreen = null)
    {
        MainWindow window = sp.GetRequiredService<MainWindow>();
        window.SplashScreen = splashScreen;
        registry.Register(window);
        return window;
    }
}