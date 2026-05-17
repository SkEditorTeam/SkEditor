using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SkEditor.Models.Welcome;

namespace SkEditor.Contracts.Services.Files;

public interface IRecentFileService
{
    ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems { get; }

    void AddRecentItem(string name, string path, bool isFolder, DateTime lastAccessed);
    void RemoveRecentItem(string path);
    void UpdateRecentItem(string? oldPath, string newPath);

    Task LoadRecentItemsAsync();
    Task SaveRecentItemsAsync();
}