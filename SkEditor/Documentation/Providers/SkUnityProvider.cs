using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Configuration.Configs;
using SkEditor.Documentation.Models;

namespace SkEditor.Documentation.Providers;

public class SkUnityProvider(SecretsConfig secrets) : IDocumentationProvider
{
    public string Name => "skUnity";
    public string FileName => "skunity.json";

    public string GetDownloadUrl()
    {
        string apiKey = secrets.SkUnityApiKey;

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            throw new DocumentationException
            {
                ApiKeyMissing = true
            };
        }

        return $"https://api.skunity.com/v1/{apiKey}/docs/getAllSyntax";
    }

    public async Task<IReadOnlyList<DocumentationEntry>> GetDocumentationEntriesAsync(Stream jsonStream,
        CancellationToken ct)
    {
        using (JsonDocument doc = await JsonDocument.ParseAsync(jsonStream, cancellationToken: ct))
        {
            JsonElement root = doc.RootElement;
            if (root.TryGetProperty("response", out JsonElement resp) && resp.ValueEquals("failed"))
            {
                throw new DocumentationException
                {
                    ApiKeyInvalid = true
                };
            }
        }

        jsonStream.Position = 0;
        
        SkUnityResponseDto? response =
            await JsonSerializer.DeserializeAsync<SkUnityResponseDto>(jsonStream,
                SkUnityResponseContext.Default.SkUnityResponseDto, ct);

        if (response is null)
        {
            return [];
        }

        var entries = response.ResultDto.Docs.Values.Select(doc => new DocumentationEntry
        {
            Name = WebUtility.HtmlDecode(doc.Name),
            Type = StringToCategory(doc.Type),
            Description = WebUtility.HtmlDecode(doc.Description).Replace("\\n", Environment.NewLine),
            Addon = WebUtility.HtmlDecode(doc.Addon),
            Version = WebUtility.HtmlDecode(doc.Version),
            Patterns = WebUtility.HtmlDecode(doc.Pattern)
        }).ToList();

        return entries;
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