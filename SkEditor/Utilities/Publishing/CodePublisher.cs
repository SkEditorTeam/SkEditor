using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Controls;
using SkEditor.API;
using PublishWindow = SkEditor.Views.Windows.PublishWindow;

namespace SkEditor.Utilities.Publishing;

public class CodePublisher
{
    public static async Task PublishPastebin(string code, PublishWindow window)
    {
        try
        {
            HttpClient client = new();
            HttpRequestMessage request = new(HttpMethod.Post, "https://pastebin.com/api/api_post.php");
            List<KeyValuePair<string, string>> collection =
            [
                new("api_dev_key", window.ApiKey ?? ""),
                new("api_paste_code", code),
                new("api_option", "paste")
            ];
            FormUrlEncodedContent content = new(collection);
            request.Content = content;
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            window.ResultTextBox.Text = responseString;
        }
        catch (Exception e)
        {
            await SkEditorAPI.Windows.ShowError(
                "Something went wrong.\nDo you have an internet connection? Did you enter correct API key?\n\n" +
                e.Message);
        }
    }

    public static async Task PublishCodeSkriptPl(string code, PublishWindow window)
    {
        string? language = (window.LanguageComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString()?.ToLower();

        try
        {
            string json = JsonSerializer.Serialize(new CodeSkriptPlCreateRequest
            {
                Key = window.ApiKey ?? "",
                Language = language,
                Content = code,
                Anonymous = window.AnonymouslyCheckBox.IsChecked ?? false
            }, PublishingJsonContext.Default.CodeSkriptPlCreateRequest);

            HttpClient client = new();
            HttpResponseMessage response = await client.PostAsync("https://code.skript.pl/api/v1/codes/create",
                new StringContent(json, Encoding.UTF8, "application/json"));
            string responseString = await response.Content.ReadAsStringAsync();

            CodeSkriptPlCreateResponse? result = JsonSerializer.Deserialize<CodeSkriptPlCreateResponse>(responseString,
                PublishingJsonContext.Default.CodeSkriptPlCreateResponse);

            string? url = result?.Url;
            window.ResultTextBox.Text = url;
        }
        catch (Exception e)
        {
            await SkEditorAPI.Windows.ShowError(
                "Something went wrong.\nDo you have an internet connection? Did you enter correct API key?\n\n" +
                e.Message);
        }
    }

    public static async Task PublishSkUnity(string code, PublishWindow window)
    {
        try
        {
            string? apiKey = window.ApiKey;

            string json = JsonSerializer.Serialize(new SkUnityCreateRequest { Content = code },
                PublishingJsonContext.Default.SkUnityCreateRequest);
            string? fileName = SkEditorAPI.Files.GetCurrentOpenedFile()?.Name;

            HttpClient client = new();
            HttpRequestMessage request = new(HttpMethod.Post,
                $"https://api.skunity.com/v1/{apiKey}/parser/savenewfile/{fileName}");
            request.Headers.Add("User-Agent", "SkEditor");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            SkUnityCreateResponse? jsonResult = JsonSerializer.Deserialize<SkUnityCreateResponse>(responseString,
                PublishingJsonContext.Default.SkUnityCreateResponse);
            string url = "https://parser.skunity.com/" + jsonResult?.Result.ShareCode;
            window.ResultTextBox.Text = url;
        }
        catch (Exception e)
        {
            await SkEditorAPI.Windows.ShowError(
                "Something went wrong.\nDo you have an internet connection? Did you enter correct API key?\n\n" +
                e.Message);
        }
    }
}

[JsonSerializable(typeof(CodeSkriptPlCreateRequest))]
[JsonSerializable(typeof(CodeSkriptPlCreateResponse))]
[JsonSerializable(typeof(SkUnityCreateRequest))]
[JsonSerializable(typeof(SkUnityCreateResponse))]
public partial class PublishingJsonContext : JsonSerializerContext;

public class CodeSkriptPlCreateRequest
{
    [JsonPropertyName("key")] public required string Key { get; set; }
    [JsonPropertyName("language")] public string? Language { get; set; }
    [JsonPropertyName("content")] public required string Content { get; set; }
    [JsonPropertyName("anonymous")] public required bool Anonymous { get; set; }
}

public class CodeSkriptPlCreateResponse
{
    [JsonPropertyName("url")] public required string Url { get; set; }
}

public class SkUnityCreateRequest
{
    [JsonPropertyName("content")] public required string Content { get; set; }
}

public class SkUnityCreateResponse
{
    [JsonPropertyName("result")] public required SkUnityCreateResult Result { get; set; }
}

public class SkUnityCreateResult
{
    [JsonPropertyName("share_code")] public required string ShareCode { get; set; }
}