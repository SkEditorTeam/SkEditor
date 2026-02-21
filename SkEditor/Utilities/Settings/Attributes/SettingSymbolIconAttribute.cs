using System;
using FluentIcons.Common;

namespace SkEditor.Utilities.Settings.Attributes;
#pragma warning disable CS9113 // Parameter is unread.

/// <summary>
///     Specifies the symbol icon for a setting.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SettingSymbolIconAttribute(Symbol symbol) : Attribute;