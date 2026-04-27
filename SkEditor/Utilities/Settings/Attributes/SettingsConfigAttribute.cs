using System;

namespace SkEditor.Utilities.Settings.Attributes;

/// <summary>
///     Marks a class as a settings config to generate all the boilerplate code.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class SettingsConfigAttribute(bool generateCopyFrom = true) : Attribute;