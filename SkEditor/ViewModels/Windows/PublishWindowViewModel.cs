using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Integrations;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.ViewModels.Windows;

public partial class PublishWindowViewModel : ObservableObject
{
    public PublishWindowViewModel(IPublishService publishService, INotificationService notifications,
        IClipboardService clipboard, SecretsConfig secretsConfig)
    {
        SelectedType = PublishTypes[0];
        SelectedLanguage = Languages[0];

        PublishCommand = new AsyncRelayCommand(async () =>
        {
            string code = "Test code, todo: change it!";

            string? apiKey = SelectedType switch
            {
                { Type: PublishType.Pastebin } => secretsConfig.PastebinApiKey,
                { Type: PublishType.SkUnity } => secretsConfig.SkUnityApiKey,
                { Type: PublishType.CodeSkriptPl } => secretsConfig.CodeSkriptPlApiKey,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                notifications.ShowError("API key missing",
                    "The selected publish type requires an API key, but it is not set. Please set the API key in the settings and try again.");
                return;
            }

            string language = SelectedLanguage.ToLower();
            string? result = await publishService.PublishCode(code, apiKey, SelectedType.Type, language, false);
            if (result is null)
            {
                notifications.ShowError("Failed to publish code",
                    "An error occurred while trying to publish the code. See logs for more details.");
                return;
            }

            Result = result;
        });

        CopyCommand = new AsyncRelayCommand(async () =>
        {
            if (string.IsNullOrWhiteSpace(Result))
            {
                return;
            }

            await clipboard.SetTextAsync(Result);
        });
    }

    [ObservableProperty]
    public partial string Result { get; private set; } = string.Empty;

    public List<PublishTypeEntry> PublishTypes { get; } =
    [
        new(PublishType.Pastebin, "Pastebin"),
        new(PublishType.SkUnity, "skUnity Parser"),
        new(PublishType.CodeSkriptPl, "code.skript.pl")
    ];

    public List<string> Languages { get; } =
    [
        "Skript", "YAML", "Text"
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AreExtraFlagsSupported))]
    public partial PublishTypeEntry SelectedType { get; set; }

    [ObservableProperty]
    public partial string SelectedLanguage { get; set; }

    public bool AreExtraFlagsSupported => SelectedType.Type == PublishType.CodeSkriptPl;

    public IRelayCommand PublishCommand { get; }
    public IRelayCommand CopyCommand { get; }

    public record PublishTypeEntry(PublishType Type, string Name);
}