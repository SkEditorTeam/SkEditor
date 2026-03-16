using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.API;

namespace SkEditor.ViewModels.Pages;

public class WhatsNewPageViewModel : ObservableObject
{
    public string Version { get; } = $"v{SkEditorAPI.Core.GetInformationalVersion()}";
}