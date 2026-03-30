using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Integrations;

namespace SkEditor.ViewModels.Windows;

public partial class PublishWindowViewModel : ObservableObject
{
    public PublishWindowViewModel(IPublishService publishService, INotificationService notifications,
        IClipboardService clipboard)
    {
        SelectedType = PublishTypes[0];
        SelectedLanguage = Languages[0];

        PublishCommand = new AsyncRelayCommand(async () =>
        {
            string? result = await publishService.PublishCode(SelectedType.Type);
            if (result is null)
            {
                notifications.Debug("HELLO WORLD");
                return;
            }
        });

        CopyCommand = new AsyncRelayCommand(async () =>
        {
            // if (string.IsNullOrWhiteSpace(Result))
            // {
            //     return;
            // }

            await clipboard.SetTextAsync($"TEST" + Random.Shared.Next(1000, 9999));
        });
    }

    public string Result { get; private set; } = string.Empty;

    public List<PublishTypeEntry> PublishTypes { get; } =
    [
        new(PublishType.Pastebin, "Pastebin"),
        new(PublishType.SkUnity, "skUnity Parser"),
        new(PublishType.CodeSkriptPl, "code.skript.pl")
    ];

    public List<string> Languages { get; } =
    [
        "Skript", "YAML"
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