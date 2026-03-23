using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.API;
using SkEditor.Contracts.Services;
using SkEditor.Models.Welcome;

namespace SkEditor.ViewModels.Pages;

public class WelcomePageViewModel(IWelcomeService welcomeService) : ObservableObject
{
    public string Version { get; } = $"v{SkEditorAPI.Core.GetInformationalVersion()}";

    public ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems => welcomeService.RecentItems;
    public ObservableCollection<WelcomeScreenSection> WelcomeSections => welcomeService.WelcomeSections;
}