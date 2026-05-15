using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;
using Avalonia.Media.Immutable;

namespace SkEditor.Converters;

public class ImmutableSolidColorBrushConverter : JsonConverter<ImmutableSolidColorBrush>
{
    public override ImmutableSolidColorBrush? Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        string? hex = reader.GetString();
        if (string.IsNullOrWhiteSpace(hex))
        {
            return null;
        }

        try
        {
            return new ImmutableSolidColorBrush(Color.Parse(hex));
        }
        catch
        {
            return new ImmutableSolidColorBrush(Colors.Magenta);
        }
    }

    public override void Write(Utf8JsonWriter writer, ImmutableSolidColorBrush value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Color.ToString());
    }
}