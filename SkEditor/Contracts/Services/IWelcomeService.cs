using System.Collections.ObjectModel;
using SkEditor.Models.Welcome;

namespace SkEditor.Contracts.Services;

/// <summary>
///     Provides data for the welcome screen.
/// </summary>
public interface IWelcomeService
{
    /// <summary>
    ///     Gets the recent items to display on the welcome screen.
    /// </summary>
    ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems { get; }

    /// <summary>
    ///     Gets the sections to display on the welcome screen.
    /// </summary>
    ObservableCollection<WelcomeScreenSection> WelcomeSections { get; }
}