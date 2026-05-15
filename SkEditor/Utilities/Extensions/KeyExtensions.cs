using System;
using Avalonia.Input;

namespace SkEditor.Utilities.Extensions;

public static class KeyExtensions
{
    extension(KeyModifiers)
    {
        /// <summary>
        ///     Gets the platform-specific modifier key used for shortcuts (Control on Windows/Linux, Command on macOS).
        /// </summary>
        public static KeyModifiers Command =>
            OperatingSystem.IsMacOS() ? KeyModifiers.Meta : KeyModifiers.Control;
    }
}