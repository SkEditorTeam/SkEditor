using System;
using System.Text.Json.Nodes;
using Avalonia.Controls;

namespace SkEditor.API.Settings.Types;

/// <summary>
///     Represent a setting that can be toggled on or off.
/// </summary>
public class ToggleSetting : ISettingType
{
    public object Deserialize(JsonNode? value)
    {
        return value?.GetValue<bool>() ?? false;
    }

    public JsonNode? Serialize(object value)
    {
        return JsonValue.Create((bool)value);
    }

    public Control CreateControl(object raw, Action<object> onChanged)
    {
        bool value = (bool)raw;
        ToggleSwitch toggle = new() { IsChecked = value };
        toggle.IsCheckedChanged += (_, _) => onChanged(toggle.IsChecked ?? false);
        return toggle;
    }

    public bool IsSelfManaged => false;
}