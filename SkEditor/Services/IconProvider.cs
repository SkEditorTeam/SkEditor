using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Platform;

namespace SkEditor.Services;

public class IconProvider
{
    private readonly Dictionary<string, FileIconEntry> _iconMap = new();
    public readonly FileIconEntry DefaultIcon = new("Document", "#CFD8DC", []);

    public async Task LoadIcons()
    {
        Uri iconMapJsonFile = new("avares://SkEditor/Assets/IconMap.json");
        await using Stream stream = AssetLoader.Open(iconMapJsonFile);

        FileIconEntry[]? entries = JsonSerializer.Deserialize<FileIconEntry[]>(
            await new StreamReader(stream).ReadToEndAsync(), FileIconMapContext.Default.FileIconEntryArray);

        if (entries is null)
        {
            throw new InvalidOperationException("Failed to load icon map: deserialized entries are null.");
        }

        foreach (FileIconEntry entry in entries)
        {
            foreach (string ext in entry.Extensions)
            {
                string lowerExt = ext.ToLower();
                _iconMap.TryAdd(lowerExt, entry);
            }
        }
    }

    public FileIconEntry GetIconForExtension(string extension)
    {
        string lowerExt = extension.ToLower();
        return _iconMap.GetValueOrDefault(lowerExt, DefaultIcon);
    }

    public class FileIconEntry(string icon, string color, string[] extensions)
    {
        [JsonPropertyName("icon")]
        public string Icon { get; init; } = icon;

        [JsonPropertyName("color")]
        public string Color { get; init; } = color;

        [JsonPropertyName("extensions")]
        public string[] Extensions { get; init; } = extensions;
    }
}

[JsonSerializable(typeof(IconProvider.FileIconEntry))]
[JsonSerializable(typeof(IconProvider.FileIconEntry[]))]
public partial class FileIconMapContext : JsonSerializerContext;