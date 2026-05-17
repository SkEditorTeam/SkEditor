using FluentAvalonia.UI.Windowing;
using SkEditor.Views.Windows;

namespace SkEditor.Utilities.Styling;

public static class WindowStyler
{
    private const int TitleBarHeight = 50;

    public static void Style(this FAAppWindow window)
    {
        if (window is MainWindow)
        {
            window.TitleBar.Height = TitleBarHeight;
        }
    }
}