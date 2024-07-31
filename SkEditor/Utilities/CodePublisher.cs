using Avalonia.Controls;
using Newtonsoft.Json.Linq;
using SkEditor.API;
using SkEditor.Views;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SkEditor.Utilities
{
    public class CodePublisher
    {
        public static async Task PublishPastebin(string code, PublishWindow window)
        {
            try
            {
                using HttpClient client = new();
                var request = new HttpRequestMessage(HttpMethod.Post, "https://pastebin.com/api/api_post.php");
                var collection = new List<KeyValuePair<string, string>>
                {
                    new("api_dev_key", window.ApiKeyTextBox.Text),
                    new("api_paste_code", code),
                    new("api_option", "paste")
                };
                var content = new FormUrlEncodedContent(collection);
                request.Content = content;
                var response = await client.SendAsync(request);
                string responseString = await response.Content.ReadAsStringAsync();

                string url = responseString;
                window.ResultTextBox.Text = url;
            }
            catch (Exception e)
            {
                await SkEditorAPI.Windows.ShowError("Something went wrong.\nDo you have an internet connection? Did you enter correct API key?\n\n" + e.Message);
            }
        }

        public static async Task PublishCodeSkriptPl(string code, PublishWindow window)
        {
            string language = (window.LanguageComboBox.SelectedItem as ComboBoxItem).Content.ToString().ToLower();
            bool? anonymousChecked = window.AnonymouslyCheckBox.IsChecked;
            string directory = !anonymousChecked.HasValue || !anonymousChecked.Value ? "SkEditor_content" : null;

            try
            {
                string json = JsonSerializer.Serialize(new
                {
                    key = window.ApiKeyTextBox.Text,
                    language,
                    content = code,
                    anonymous = anonymousChecked,
                    directory
                });

                using HttpClient client = new();
                HttpResponseMessage response = await client.PostAsync("https://code.skript.pl/api/v1/codes/create",
                                                new StringContent(json, System.Text.Encoding.UTF8, "application/json"));
                string responseString = await response.Content.ReadAsStringAsync();

                JObject jsonResult = JObject.Parse(responseString);
                string url = jsonResult["url"].ToString();
                window.ResultTextBox.Text = url;
            }
            catch (Exception e)
            {
                await SkEditorAPI.Windows.ShowError("Something went wrong.\nDo you have an internet connection? Did you enter correct API key?\n\n" + e.Message);
            }
        }

        public static async Task PublishSkUnity(string code, PublishWindow window)
        {
            try
            {
                string apiKey = window.ApiKeyTextBox.Text;
                string json = JsonSerializer.Serialize(new { content = code });
                string fileName = SkEditorAPI.Files.GetCurrentOpenedFile().Name;

                using HttpClient client = new();
                HttpRequestMessage request = new(HttpMethod.Post, $"https://api.skunity.com/v1/{apiKey}/parser/savenewfile/{fileName}");
                request.Headers.Add("User-Agent", "SkEditor");
                request.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                string responseString = await response.Content.ReadAsStringAsync();

                JObject jsonResult = JObject.Parse(responseString);
                string url = "https://parser.skunity.com/" + jsonResult["result"]["share_code"].ToString();
                window.ResultTextBox.Text = url;
            }
            catch (Exception e)
            {
                await SkEditorAPI.Windows.ShowError("Something went wrong.\nDo you have an internet connection? Did you enter correct API key?\n\n" + e.Message);
            }
        }
    }
}
