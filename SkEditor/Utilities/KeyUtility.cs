using System;
using Avalonia.Input;

namespace SkEditor.Utilities;

public static class KeyUtility
{
    public static KeyModifiers GetControlModifier()
    {
        return OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
    }
}