using System;

namespace SkEditor.Utilities.Styling.Attributes;

[AttributeUsage(AttributeTargets.Property)]

public class ThemeResourceAttribute(params string[] keys) : Attribute
{
    public string[] Keys { get; } = keys;
    public bool MapPropertyName { get; set; } = true;
}