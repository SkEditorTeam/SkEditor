using System.Collections.ObjectModel;
using SkEditor.Models.Welcome;

namespace SkEditor.Contracts.Services;

public interface IWelcomeService
{
    ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems { get; }
    ObservableCollection<WelcomeScreenSection> WelcomeSections { get; }
}