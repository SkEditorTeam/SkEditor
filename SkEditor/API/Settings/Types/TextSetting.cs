using System;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;

namespace SkEditor.API.Settings.Types;

/// <summary>
///     Represent a text input setting.
/// </summary>
/// <param name="placeholder">The placeholder text to display in the text box.</param>
/// <param name="maxLength">The maximum length (in number of char) of the text box.</param>
/// <param name="passwordChar">The character to display in place of the actual text, set to null for normal text box.</param>
/// <param name="boxWidth">The maximum width of the text box, defaulting to 200</param>
public class TextSetting(string placeholder, int maxLength = -1, char? passwordChar = null, int boxWidth = 200)
    : ISettingType
{
    public string Placeholder { get; } = placeholder;
    public int MaxLength { get; } = maxLength;
    public char? PasswordChar { get; } = passwordChar;

    public object? Deserialize(JToken value)
    {
        return value.Value<string>();
    }

    public JToken Serialize(object value)
    {
        return new JValue(value);
    }

    public Control CreateControl(object raw, Action<object> onChanged)
    {
        string value = (string)raw;

        TextBox textBox = new()
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

    public bool IsSelfManaged => false;
}