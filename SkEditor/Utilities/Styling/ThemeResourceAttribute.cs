using System;

namespace SkEditor.Utilities.Styling;

/// <summary>
///     Marks a property as a theme resource with optional keys.
/// </summary>
/// <param name="keys">The keys associated with the theme resource.</param>
[AttributeUsage(AttributeTargets.Property)]
public class ThemeResourceAttribute(params string[] keys) : Attribute
{
    /// <summary>
    ///     Gets the keys associated with the theme resource.
    /// </summary>
    public string[] Keys { get; } = keys;

    /// <summary>
    ///     Indicates whether to map the property name as a key.
    /// </summary>
    public bool MapPropertyName { get; set; } = true;
}