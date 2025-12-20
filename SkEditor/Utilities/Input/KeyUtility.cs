using System;
using Avalonia.Input;

namespace SkEditor.Utilities.Input;

public static class KeyUtility
{
    // TODO: this is pretty much unused in the app, but it probably SHOULD be used everywhere for a native feel on macOS
    public static KeyModifiers GetControlModifier()
    {
        return OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
    }
}