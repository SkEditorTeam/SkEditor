using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Integrations;
using SkEditor.Models.Notifications;
using SkEditor.Models.Tabs;
using SkEditor.Utilities.Localization;
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
                Notification notification = Notification
                    .Create(TranslatableString.FromRawString("No file selected"))
                    .WithMessage(TranslatableString.FromRawString(
                        "You need to have a file open and selected to publish its code. Please open a file and try again."))
                    .WithType(NotificationType.Error);

                notifications.Show(notification);
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
                Notification notification = Notification
                    .Create(TranslatableString.FromRawString("API key missing"))
                    .WithMessage(TranslatableString.FromRawString(
                        "The selected publish type requires an API key, but it is not set. Please set the API key in the settings and try again."))
                    .WithType(NotificationType.Error);

                notifications.Show(notification);
                return;
            }

            string code = fileTab.Document.Text;

            string? result = await publishService.PublishCodeAsync(code, apiKey, SelectedType.Type);
            if (result is null)
            {
                Notification notification = Notification
                    .Create(TranslatableString.FromRawString("Failed to publish code"))
                    .WithMessage(TranslatableString.FromRawString(
                        "An error occurred while trying to publish the code. Please check your API key and internet connection, and try again."))
                    .WithType(NotificationType.Error);

                notifications.Show(notification);
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

            await clipboard.CopyTextAsync(Result);
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

    public IRelayCommand PublishCommand { get; }
    public IRelayCommand CopyCommand { get; }

    public record PublishTypeEntry(PublishType Type, string Name);
}