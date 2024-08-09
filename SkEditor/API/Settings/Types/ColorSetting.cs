using Avalonia.Controls;
using Avalonia.Media;
using Newtonsoft.Json.Linq;
using System;

namespace SkEditor.API.Settings.Types;

public class ColorSetting : ISettingType
{
    public object Deserialize(JToken value)
    {
        if (value is not JObject obj)
            throw new ArgumentException("Value is not a JObject", nameof(value));

        return Color.FromArgb(
            obj["A"]?.Value<byte>() ?? 255,
            obj["R"]?.Value<byte>() ?? 0,
            obj["G"]?.Value<byte>() ?? 0,
            obj["B"]?.Value<byte>() ?? 0
        );
    }

    public JToken Serialize(object value)
    {
        if (value is not Color color)
            throw new ArgumentException("Value is not a Color", nameof(value));

        return new JObject
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
            throw new ArgumentException("Value is not a Color", nameof(value));

        var picker = new ColorPicker
        {
            Color = color
        };

        picker.ColorChanged += (_, _) => onChanged(picker.Color);
        return picker;
    }

    public bool IsSelfManaged => false;
}