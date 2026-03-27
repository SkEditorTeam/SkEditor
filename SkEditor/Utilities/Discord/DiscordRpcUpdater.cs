using System.Text.RegularExpressions;
using DiscordRPC;

namespace SkEditor.Utilities.Discord;

public static partial class DiscordRpcUpdater
{
    private const string ApplicationId = "1152625623777431662";
    private static DiscordRpcClient? _client;

    public static void Initialize()
    {
        // if (!SkEditorAPI.Configs.GetAppSettings().IsDiscordRichPresenceEnabled)
        // {
        //     return;
        // }

        _client = new DiscordRpcClient(ApplicationId);
        _client.Initialize();

        _client.SetPresence(new RichPresence
        {
            Timestamps = Timestamps.Now,
            Assets = new Assets
            {
                LargeImageKey = "image_large",
                LargeImageText = "SkEditor"
            }
        });

        // TabView? tabView = SkEditorAPI.Files.GetTabView();
        //
        // if (tabView == null)
        // {
        //     return;
        // }
        //
        // tabView.SelectionChanged += (_, _) =>
        // {
        //     if (!_client.IsInitialized)
        //     {
        //         return;
        //     }
        //
        //     if (SkEditorAPI.Files.IsFileOpen())
        //     {
        //         if (tabView.SelectedItem is not TabViewItem tab)
        //         {
        //             return;
        //         }
        //
        //         string tabName = tab.Header?.ToString()?.TrimEnd('*') ?? "";
        //         tabName = UnicodeControlCharacterFilter().Replace(tabName, string.Empty);
        //         string translation = Translation.Get(TranslationKey.DiscordRpcEditing, tabName);
        //         if (translation.Length > 128)
        //         {
        //             translation = translation[..125] + "...";
        //         }
        //
        //         _client.UpdateDetails(translation);
        //     }
        //     else
        //     {
        //         _client.UpdateDetails("");
        //     }
        // };
    }

    public static void Uninitialize()
    {
        if (_client == null)
        {
            return;
        }

        _client.ClearPresence();
        _client.Dispose();
    }

    [GeneratedRegex(@"[^\P{C}\n]+")]
    private static partial Regex UnicodeControlCharacterFilter();
}