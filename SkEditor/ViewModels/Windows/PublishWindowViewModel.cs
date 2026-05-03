using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Integrations;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.ViewModels.Windows;

public partial class PublishWindowViewModel : ObservableObject
{
    public PublishWindowViewModel(IPublishService publishService, INotificationService notifications,
        IClipboardService clipboard, SecretsConfig secretsConfig, ITabService tabs)
    {
        SelectedType = PublishTypes[0];

        PublishCommand = new AsyncRelayCommand(async () =>
        {
            if (tabs.SelectedTab is not FileTab fileTab)
            {
                notifications.ShowError("No file selected",
                    "You need to have a file open and selected to publish its code. Please open a file and try again.");
                return;
            }

            string? apiKey = SelectedType switch
            {
                { Type: PublishType.Pastebin } => secretsConfig.PastebinApiKey,
                { Type: PublishType.SkUnity } => secretsConfig.SkUnityApiKey,
                _ => null
            };

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                notifications.ShowError("API key missing",
                    "The selected publish type requires an API key, but it is not set. Please set the API key in the settings and try again.");
                return;
            }

            string code = fileTab.Document.Text;

            string? result = await publishService.PublishCodeAsync(code, apiKey, SelectedType.Type);
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
        new(PublishType.SkUnity, "skUnity Parser")
    ];

    [ObservableProperty]
    public partial PublishTypeEntry SelectedType { get; set; }

    [ObservableProperty]
    public partial string SelectedLanguage { get; set; }

    public IRelayCommand PublishCommand { get; }
    public IRelayCommand CopyCommand { get; }

    public record PublishTypeEntry(PublishType Type, string Name);
}