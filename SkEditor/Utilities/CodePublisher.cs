using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using SkEditor.API;
using SkEditor.Views;

namespace SkEditor.Utilities;

public class CodePublisher
{
    public static async Task PublishPastebin(string code, PublishWindow window)
    {
        try
        {
            HttpClient client = new();
            HttpRequestMessage request = new(HttpMethod.Post, "https://pastebin.com/api/api_post.php");
            List<KeyValuePair<string, string>> collection = new()
            {
                new KeyValuePair<string, string>("api_dev_key", window.ApiKeyTextBox.Text),
                new KeyValuePair<string, string>("api_paste_code", code),
                new KeyValuePair<string, string>("api_option", "paste")
            };
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
        string language = (window.LanguageComboBox.SelectedItem as ComboBoxItem).Content.ToString().ToLower();

        try
        {
            string json = JsonSerializer.Serialize(new
            {
                key = window.ApiKeyTextBox.Text,
                language,
                content = code,
                anonymous = window.AnonymouslyCheckBox.IsChecked
            });

            HttpClient client = new();
            HttpResponseMessage response = await client.PostAsync("https://code.skript.pl/api/v1/codes/create",
                new StringContent(json, Encoding.UTF8, "application/json"));
            string responseString = await response.Content.ReadAsStringAsync();

            JObject jsonResult = JObject.Parse(responseString);
            string url = jsonResult["url"].ToString();
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
            string apiKey = window.ApiKeyTextBox.Text;
            string json = JsonSerializer.Serialize(new { content = code });
            string fileName = SkEditorAPI.Files.GetCurrentOpenedFile().Name;

            HttpClient client = new();
            HttpRequestMessage request = new(HttpMethod.Post,
                $"https://api.skunity.com/v1/{apiKey}/parser/savenewfile/{fileName}");
            request.Headers.Add("User-Agent", "SkEditor");
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();

            JObject jsonResult = JObject.Parse(responseString);
            string url = "https://parser.skunity.com/" + jsonResult["result"]["share_code"];
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