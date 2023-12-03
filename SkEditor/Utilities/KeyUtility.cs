using Avalonia.Input;
using System;
using System.Runtime.InteropServices;

namespace SkEditor.Utilities;
public static class KeyUtility
{
	public static KeyModifiers GetControlModifier() => OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
}
