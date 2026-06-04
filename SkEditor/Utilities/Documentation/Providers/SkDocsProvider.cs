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

public class SkDocsProvider : IDocumentationProvider
{
    public async Task<IReadOnlyList<DocumentationEntry>> GetDocumentationEntriesAsync()
    {
        Uri jsonFile = new("avares://SkEditor/Assets/skdocs.json");
        await using Stream stream = AssetLoader.Open(jsonFile);

        SkDocsResponseDto? response =
            await JsonSerializer.DeserializeAsync<SkDocsResponseDto>(stream,
                SkDocsResponseContext.Default.SkDocsResponseDto);

        if (response is null)
        {
            return [];
        }

        return response.Results.Select(doc => new DocumentationEntry
        {
            Name = WebUtility.HtmlDecode(doc.Title),
            Type = StringToCategory(doc.Category),
            Description = WebUtility.HtmlDecode(doc.Description)?.Replace("\\n", Environment.NewLine),
            Addon = WebUtility.HtmlDecode(doc.Addon),
            Version = WebUtility.HtmlDecode(doc.Since),
            Patterns = WebUtility.HtmlDecode(doc.Syntax)
        }).ToList();
    }

    private static DocumentationSyntaxType StringToCategory(string type)
    {
        return type.ToLowerInvariant() switch
        {
            "event" => DocumentationSyntaxType.Event,
            "effect" => DocumentationSyntaxType.Effect,
            "expression" => DocumentationSyntaxType.Expression,
            "condition" => DocumentationSyntaxType.Condition,
            "type" => DocumentationSyntaxType.Type,
            "structure" => DocumentationSyntaxType.Structure,
            "section" => DocumentationSyntaxType.Section,
            "function" => DocumentationSyntaxType.Function,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}