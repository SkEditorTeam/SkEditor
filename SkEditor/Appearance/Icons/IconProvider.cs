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
    public readonly FileIconEntry DefaultIcon = new("Document", "#CFD8DC", []);
    private readonly Dictionary<string, FileIconEntry> _iconMap = new();
    private readonly Dictionary<string, string> _idColorMap = new();

    public async Task LoadIcons(Dictionary<string, string> themeColors)
    {
        Uri iconMapJsonFile = new("avares://SkEditor/Assets/IconMap.json");
        await using Stream stream = AssetLoader.Open(iconMapJsonFile);

        FileIconEntryDto[]? entries = JsonSerializer.Deserialize<FileIconEntryDto[]>(
            await new StreamReader(stream).ReadToEndAsync(), FileIconMapContext.Default.FileIconEntryDtoArray);

        if (entries is null)
        {
            throw new InvalidOperationException("Failed to load icon map: deserialized entries are null.");
        }

        _iconMap.Clear();
        foreach (FileIconEntryDto entry in entries)
        {
            string color = themeColors.GetValueOrDefault(entry.Id, "#CFD8DC");
            foreach (string ext in entry.Extensions)
            {
                string lowerExt = ext.ToLower();

                FileIconEntry iconEntry = new(entry.Icon, color, entry.Extensions);
                _iconMap.Add(lowerExt, iconEntry);
            }

            _idColorMap[entry.Id] = color;
        }
    }

    public FileIconEntry GetIconForExtension(string extension)
    {
        string lowerExt = extension.ToLower();
        return _iconMap.GetValueOrDefault(lowerExt, DefaultIcon);
    }

    public string GetColorForId(string id)
    {
        return _idColorMap.GetValueOrDefault(id, "#CFD8DC");
    }

    public class FileIconEntryDto
    {
        [JsonPropertyName("extensions")]
        public required string[] Extensions { get; init; }

        [JsonPropertyName("icon")]
        public required string Icon { get; init; }

        [JsonPropertyName("id")]
        public required string Id { get; set; }
    }

    public class FileIconEntry(string icon, string color, string[] extensions)
    {
        public string Color { get; } = color;
        public string[] Extensions { get; } = extensions;
        public string Icon { get; } = icon;
    }
}

[JsonSerializable(typeof(IconProvider.FileIconEntryDto))]
[JsonSerializable(typeof(IconProvider.FileIconEntryDto[]))]
public partial class FileIconMapContext : JsonSerializerContext;