using System;
using FluentIcons.Common;

namespace SkEditor.Utilities.Settings.Attributes;

/// <summary>
///     Specifies the symbol icon for a setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingSymbolIconAttribute(Symbol symbol) : Attribute
{
    public Symbol Symbol { get; } = symbol;
}