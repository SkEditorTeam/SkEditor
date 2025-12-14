using Avalonia.Media;
using FluentAvalonia.UI.Windowing;
using SkEditor.Views.Windows;

namespace SkEditor.Utilities.Styling;

public static class WindowStyler
{
    private static readonly Color ButtonHoverColor = Color.Parse("#25ffffff");
    private static readonly Color ButtonPressedColor = Color.Parse("#20ffffff");
    private static readonly Color ButtonInactiveColor = Color.Parse("#99ffffff");
    private const int TitleBarHeight = 50;

    public static void Style(AppWindow window)
    {
        window.TitleBar.ExtendsContentIntoTitleBar = true;
        window.TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        window.TitleBar.ButtonHoverBackgroundColor = ButtonHoverColor;
        window.TitleBar.ButtonPressedBackgroundColor = ButtonPressedColor;
        window.TitleBar.ButtonInactiveForegroundColor = ButtonInactiveColor;

        if (window is MainWindow)
        {
            window.TitleBar.Height = TitleBarHeight;
        }
    }
}