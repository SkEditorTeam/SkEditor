using System;
using Avalonia.Input;

namespace SkEditor.Layout.Menu;

public static class KeyGestureUtilities
{
    public static KeyModifiers PlatformControlModifier =>
        OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;

    public static KeyGesture ParsePlatform(string gestureText)
    {
        if (OperatingSystem.IsMacOS())
        {
            gestureText = gestureText.Replace("Ctrl", "Cmd");
        }

        return KeyGesture.Parse(gestureText);
    }
}