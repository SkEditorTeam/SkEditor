using System;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;

namespace SkEditor.API.Settings.Types;

public class TextSetting(string placeholder, int maxLength = -1, char? passwordChar = null, int boxWidth = 200) : ISettingType
{
    public string Placeholder { get; } = placeholder;
    public int MaxLength { get; } = maxLength;
    public char? PasswordChar { get; } = passwordChar;

    public object Deserialize(JToken value)
    {
        return value.Value<string>();
    }

    public JToken Serialize(object value)
    {
        return new JValue(value);
    }

    public Control CreateControl(object raw, Action<object> onChanged)
    {
        var value = (string) raw;
        
        var textBox = new TextBox
        {
            Text = value,
            Watermark = Placeholder,
            MaxLength = MaxLength,
            PasswordChar = PasswordChar ?? '\0',
            Width = boxWidth
        };
        textBox.TextChanged += (_, _) => onChanged(textBox.Text);
        return textBox;
    }
    
}