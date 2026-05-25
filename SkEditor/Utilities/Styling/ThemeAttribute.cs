using System;

namespace SkEditor.Utilities.Styling;

/// <summary>
///     Marks a class as a theme definition to generate all the boilerplate code for theme mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ThemeAttribute : Attribute;