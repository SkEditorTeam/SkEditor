using Avalonia.Input;
using System;

namespace SkEditor.Utilities;
public static class KeyUtility
{
    public static KeyModifiers GetControlModifier() => OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
}
