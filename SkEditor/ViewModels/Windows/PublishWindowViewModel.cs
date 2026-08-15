using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Integrations;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.ViewModels.Windows;

public partial class PublishWindowViewModel : ObservableObject
{
    public IRelayCommand CopyCommand { get; }

    public IRelayCommand PublishCommand { get; }

    public List<PublishTypeEntry> PublishTypes { get; } =
    [
        new(PublishType.Pastebin, "Pastebin"),
        new(PublishType.SkUnity, "skUnity Parser")
    ];

    [ObservableProperty]
    public partial string Result { get; private set; } = string.Empty;

    [ObservableProperty]
    public partial PublishTypeEntry SelectedType { get; set; }

    public PublishWindowViewModel(IPublishService publishService, INotificationService notifications,
        IClipboardService clipboard, SecretsConfig secretsConfig, ITabService tabs)
    {
        SelectedType = PublishTypes[0];

        PublishCommand = new AsyncRelayCommand(async () =>
        {
            if (tabs.SelectedTab is not FileTabViewModel fileTab)
            {
                Notification notification = Notification
                    .Create(TranslationKey.NoFileNotificationTitle)
                    .WithMessage(TranslationKey.NoFileNotificationMessage)
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
                    .Create(TranslationKey.PublishWindowApiKeyMissingTitle)
                    .WithMessage(TranslationKey.PublishWindowApiKeyMissingMessage)
                    .WithType(NotificationType.Error);

                notifications.Show(notification);
                return;
            }

            string code = fileTab.Document.Text;

            string? result = await publishService.PublishCodeAsync(code, apiKey, SelectedType.Type);
            if (result is null)
            {
                Notification notification = Notification
                    .Create(TranslationKey.PublishWindowErrorTitle)
                    .WithMessage(TranslationKey.PublishWindowErrorMessage)
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

    public record PublishTypeEntry(PublishType Type, string Name);
}