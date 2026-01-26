using System;
using System.Text.Json.Nodes;
using Avalonia.Controls;
using Avalonia.Media;

namespace SkEditor.API.Settings.Types;

public class ColorSetting : ISettingType
{
    public object Deserialize(JsonNode? value)
    {
        if (value is not JsonObject obj)
        {
            throw new ArgumentException("Value is not a JsonObject", nameof(value));
        }

        return Color.FromArgb(
            obj["A"]?.GetValue<byte>() ?? 255,
            obj["R"]?.GetValue<byte>() ?? 0,
            obj["G"]?.GetValue<byte>() ?? 0,
            obj["B"]?.GetValue<byte>() ?? 0
        );
    }

    public JsonNode? Serialize(object value)
    {
        if (value is not Color color)
        {
            throw new ArgumentException("Value is not a Color", nameof(value));
        }

        return new JsonObject
        {
            ["R"] = color.R,
            ["G"] = color.G,
            ["B"] = color.B,
            ["A"] = color.A
        };
    }

    public Control CreateControl(object value, Action<object> onChanged)
    {
        if (value is not Color color)
        {
            throw new ArgumentException("Value is not a Color", nameof(value));
        }

        ColorPicker picker = new()
        {
            Color = color
        };

        picker.ColorChanged += (_, _) => onChanged(picker.Color);
        return picker;
    }

    public bool IsSelfManaged => false;
}