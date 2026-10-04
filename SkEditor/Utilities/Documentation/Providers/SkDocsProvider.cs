using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Models.Documentation;

namespace SkEditor.Utilities.Docs.Providers;

public class SkDocsProvider : IDocumentationProvider
{
    public string Name => "SkDocs";
    public string FileName => "skdocs.json";

    public string GetDownloadUrl()
    {
        return "https://api.skdocs.org/api/search?q=ALL_ADDON_SYNTAXES";
    }

    public async Task<IReadOnlyList<DocumentationEntry>> GetDocumentationEntriesAsync(Stream jsonStream,
        CancellationToken ct)
    {
        SkDocsResponseDto? serializedResponse =
            await JsonSerializer.DeserializeAsync<SkDocsResponseDto>(jsonStream,
                SkDocsResponseContext.Default.SkDocsResponseDto, ct);

        if (serializedResponse is null)
        {
            return [];
        }

        List<DocumentationEntry> entries = serializedResponse.Results.Select(doc => new DocumentationEntry
        {
            Name = WebUtility.HtmlDecode(doc.Title),
            Type = StringToCategory(doc.Category),
            Description = WebUtility.HtmlDecode(doc.Description)?.Replace("\\n", Environment.NewLine),
            Addon = WebUtility.HtmlDecode(doc.Addon),
            Version = WebUtility.HtmlDecode(doc.Since),
            Patterns = WebUtility.HtmlDecode(doc.Syntax)
        }).ToList();

        return entries;
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