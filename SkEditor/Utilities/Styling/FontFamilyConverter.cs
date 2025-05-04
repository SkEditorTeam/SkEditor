using System;
using Avalonia.Media;
using Newtonsoft.Json;

namespace SkEditor.Utilities.Styling;

public class FontFamilyConverter : JsonConverter<FontFamily?>
{
    public override FontFamily ReadJson(JsonReader reader, Type objectType, FontFamily? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.String)
        {
            return null;
        }

        string? fontFamilyName = reader.Value.ToString();
        return new FontFamily(fontFamilyName);
    }

    public override void WriteJson(JsonWriter writer, FontFamily? value, JsonSerializer serializer)
    {
        if (value == null)
        {
            return;
        }

        writer.WriteValue(value.Name);
    }
}