using System;
using FluentAvalonia.UI.Controls;
using SkEditor.API.Settings.Types;

namespace SkEditor.API.Settings;

/// <summary>
/// Represent an addon's settings.
/// </summary>
public class Setting(
    IAddon addon,
    string name,
    string key,
    object defaultValue,
    ISettingType type,
    string? description = null,
    IconSource? icon = null)
{
    
    /// <summary>
    /// The addon that owns this setting.
    /// </summary>
    public IAddon Addon { get; } = addon;
    
    /// <summary>
    /// The displayed name of the setting, in the setting expander.
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// The displayed description of the setting, in the setting expander.
    /// </summary>
    public string? Description { get; } = description;
    
    /// <summary>
    /// The key (= identifier) of the setting. It's used in
    /// the settings dictionary to store the value.
    /// </summary>
    public string Key { get; } = key;

    /// <summary>
    /// The icon displayed next to the setting name.
    /// </summary>
    public IconSource? Icon { get; } = icon;

    /// <summary>
    /// The default value of the setting.
    /// </summary>
    public object DefaultValue { get; } = defaultValue;
    
    /// <summary>
    /// Get the type of the setting.
    /// </summary>
    public ISettingType Type { get; } = type;
    
    /// <summary>
    /// Fired when the setting is changed.
    /// </summary>
    public Action<object>? OnChanged { get; set; }
}