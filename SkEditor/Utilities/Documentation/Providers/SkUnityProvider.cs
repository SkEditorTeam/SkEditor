using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform;
using SkEditor.Models.Documentation;

namespace SkEditor.Utilities.Docs.Providers;

public class SkUnityProvider : IDocumentationProvider
{
    public async Task<IReadOnlyList<DocumentationEntry>> GetDocumentationEntriesAsync()
    {
        Uri jsonFile = new("avares://SkEditor/Assets/skunity-docs.json");
        await using Stream stream = AssetLoader.Open(jsonFile);

        SkUnityResponseDto? response =
            await JsonSerializer.DeserializeAsync<SkUnityResponseDto>(stream,
                SkUnityResponseContext.Default.SkUnityResponseDto);

        if (response is null)
        {
            return [];
        }

        return response.ResultDto.Docs.Values.Select(doc => new DocumentationEntry
        {
            Name = WebUtility.HtmlDecode(doc.Name),
            Type = StringToCategory(doc.Type),
            Description = WebUtility.HtmlDecode(doc.Description).Replace("\\n", Environment.NewLine),
            Addon = WebUtility.HtmlDecode(doc.Addon),
            Version = WebUtility.HtmlDecode(doc.Version),
            Patterns = WebUtility.HtmlDecode(doc.Pattern)
        }).ToList();
    }

    private static DocumentationSyntaxType StringToCategory(string type)
    {
        type = type.ToLowerInvariant();
        if (type.EndsWith('s'))
        {
            type = type[..^1];
        }

        return type switch
        {
            "event" => DocumentationSyntaxType.Event,
            "effect" => DocumentationSyntaxType.Effect,
            "expression" => DocumentationSyntaxType.Expression,
            "condition" => DocumentationSyntaxType.Condition,
            "type" or "class" or "classe" => DocumentationSyntaxType.Type,
            "structure" => DocumentationSyntaxType.Structure,
            "section" => DocumentationSyntaxType.Section,
            "function" => DocumentationSyntaxType.Function,
            _ => DocumentationSyntaxType.Expression
        };
    }
}