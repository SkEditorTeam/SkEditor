using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Models.Integrations;

namespace SkEditor.ViewModels.Windows;

public partial class PublishWindowViewModel : ObservableObject
{
    public PublishWindowViewModel()
    {
        SelectedType = PublishTypes[0];
        SelectedLanguage = Languages[0];
    }

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

    public record PublishTypeEntry(PublishType Type, string Name);
}