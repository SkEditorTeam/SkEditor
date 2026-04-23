using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Integrations;

namespace SkEditor.Utilities.Publishing;

public class PublishService(HttpClient httpClient, ILogService logs) : IPublishService
{
    public async Task<string?> PublishCodeAsync(string code, string apiKey, PublishType type, string language,
        bool isAnonymous)
    {
        return type switch
        {
            PublishType.Pastebin => await PublishPastebin(code, apiKey),
            PublishType.CodeSkriptPl => await PublishCodeSkriptPl(code, apiKey, language, isAnonymous),
            PublishType.SkUnity => await PublishSkUnity(code, apiKey),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public async Task<string?> PublishPastebin(string code, string apiKey)
    {
        try
        {
            HttpRequestMessage request = new(HttpMethod.Post, "https://pastebin.com/api/api_post.php");
            List<KeyValuePair<string, string>> collection =
            [
                new("api_dev_key", apiKey),
                new("api_paste_code", code),
                new("api_option", "paste")
            ];
            FormUrlEncodedContent content = new(collection);
            request.Content = content;

            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }
        catch (Exception e)
        {
            logs.Error(e, "Failed to publish code to Pastebin");
        }

        return null;
    }

    public async Task<string?> PublishCodeSkriptPl(string code, string apiKey, string language, bool isAnonymous)
    {
        try
        {
            string json = JsonSerializer.Serialize(new CodeSkriptPlCreateRequest
            {
                Key = apiKey,
                Language = language,
                Content = code,
                Anonymous = isAnonymous
            }, PublishingJsonContext.Default.CodeSkriptPlCreateRequest);

            HttpResponseMessage response = await httpClient.PostAsync("https://code.skript.pl/api/v1/codes/create",
                new StringContent(json, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            CodeSkriptPlCreateResponse? result = JsonSerializer.Deserialize<CodeSkriptPlCreateResponse>(responseString,
                PublishingJsonContext.Default.CodeSkriptPlCreateResponse);

            string? url = result?.Url;
            return url;
        }
        catch (Exception e)
        {
            logs.Error(e, "Failed to publish code to code.skript.pl");
        }

        return null;
    }

    public async Task<string?> PublishSkUnity(string code, string apiKey)
    {
        try
        {
            string json = JsonSerializer.Serialize(new SkUnityCreateRequest { Content = code },
                PublishingJsonContext.Default.SkUnityCreateRequest);

            // TODO: add file name
            // string? fileName = SkEditorAPI.Files.GetCurrentOpenedFile()?.Name;
            string fileName = "skeditor-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".sk";

            HttpRequestMessage request = new(HttpMethod.Post,
                $"https://api.skunity.com/v1/{apiKey}/parser/savenewfile/{fileName}");
            request.Headers.Add("User-Agent", "SkEditor");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            string responseString = await response.Content.ReadAsStringAsync();

            SkUnityCreateResponse? jsonResult = JsonSerializer.Deserialize<SkUnityCreateResponse>(responseString,
                PublishingJsonContext.Default.SkUnityCreateResponse);

            string url = "https://parser.skunity.com/" + jsonResult?.Result.ShareCode;
            return url;
        }
        catch (Exception e)
        {
            logs.Error(e, "Failed to publish code to skUnity Parser");
        }

        return null;
    }
}

[JsonSerializable(typeof(CodeSkriptPlCreateRequest))]
[JsonSerializable(typeof(CodeSkriptPlCreateResponse))]
[JsonSerializable(typeof(SkUnityCreateRequest))]
[JsonSerializable(typeof(SkUnityCreateResponse))]
public partial class PublishingJsonContext : JsonSerializerContext;

public class CodeSkriptPlCreateRequest
{
    [JsonPropertyName("key")]
    public required string Key { get; set; }

    [JsonPropertyName("language")]
    public string? Language { get; set; }

    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("anonymous")]
    public required bool Anonymous { get; set; }
}

public class CodeSkriptPlCreateResponse
{
    [JsonPropertyName("url")]
    public required string Url { get; set; }
}

public class SkUnityCreateRequest
{
    [JsonPropertyName("content")]
    public required string Content { get; set; }
}

public class SkUnityCreateResponse
{
    [JsonPropertyName("result")]
    public required SkUnityCreateResult Result { get; set; }
}

public class SkUnityCreateResult
{
    [JsonPropertyName("share_code")]
    public required string ShareCode { get; set; }
}