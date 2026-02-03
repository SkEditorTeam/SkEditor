using System;

namespace SkEditor.Utilities.Settings.Attributes;

/// <summary>
///     Specifies the path icon for a setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingPathIconAttribute(string iconName) : Attribute;