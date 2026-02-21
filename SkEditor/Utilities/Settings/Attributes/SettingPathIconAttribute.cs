using System;

namespace SkEditor.Utilities.Settings.Attributes;
#pragma warning disable CS9113 // Parameter is unread.

/// <summary>
///     Specifies the path icon for a setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingPathIconAttribute(string iconName) : Attribute;