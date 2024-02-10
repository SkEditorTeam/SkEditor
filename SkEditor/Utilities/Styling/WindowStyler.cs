using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Windowing;

namespace SkEditor.Utilities.Styling;
public class WindowStyler
{
    public static void Style(AppWindow window)
    {
        window.TitleBar.ExtendsContentIntoTitleBar = true;
        window.TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;
        window.TitleBar.ButtonHoverBackgroundColor = Color.Parse("#25ffffff");
        window.TitleBar.ButtonPressedBackgroundColor = Color.Parse("#20ffffff");
        window.TitleBar.ButtonInactiveForegroundColor = Color.Parse("#99ffffff");

        if (ThemeEditor.CurrentTheme.UseMicaEffect) window.TransparencyLevelHint = [WindowTransparencyLevel.Mica];
    }
}
