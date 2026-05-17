using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Welcome;

namespace SkEditor.Services.Files;

public class RecentFileService : IRecentFileService
{
    private const int MaxItems = 20;
    private readonly ObservableCollection<WelcomeScreenRecentItem> _recentItems = [];
    private readonly string _savePath;
    private readonly ILogService _logs;

    public RecentFileService(IConfigService configs, ILogService logs)
    {
        _savePath = Path.Combine(configs.GetAppDataPath(), "recent.json");
        _logs = logs;

        RecentItems = new ReadOnlyObservableCollection<WelcomeScreenRecentItem>(_recentItems);
    }

    public ReadOnlyObservableCollection<WelcomeScreenRecentItem> RecentItems { get; }

    public void AddRecentItem(string name, string path, bool isFolder, DateTime lastAccessed)
    {
        WelcomeScreenRecentItem? existing = _recentItems.FirstOrDefault(i => i.Path == path);
        if (existing != null)
        {
            _recentItems.Remove(existing);
        }

        WelcomeScreenRecentItem item = new(name, path, isFolder, lastAccessed);

        _recentItems.Insert(0, item);

        while (_recentItems.Count > MaxItems)
        {
            _recentItems.RemoveAt(_recentItems.Count - 1);
        }

        _ = SaveRecentItemsAsync();
    }

    public void RemoveRecentItem(string path)
    {
        WelcomeScreenRecentItem? existing = _recentItems.FirstOrDefault(i => i.Path == path);
        if (existing != null)
        {
            _recentItems.Remove(existing);

            _ = SaveRecentItemsAsync();
        }
    }

    public void UpdateRecentItem(string? oldPath, string newPath)
    {
        WelcomeScreenRecentItem? existing = _recentItems.FirstOrDefault(i => i.Path == oldPath);
        if (existing == null)
        {
            AddRecentItem(Path.GetFileName(newPath), newPath, false, DateTime.Now);
            return;
        }

        string name = Path.GetFileName(newPath);
        WelcomeScreenRecentItem updatedItem = new(name, newPath, existing.IsFolder, DateTime.Now);
        int index = _recentItems.IndexOf(existing);
        _recentItems[index] = updatedItem;

        _ = SaveRecentItemsAsync();
    }

    public async Task LoadRecentItemsAsync()
    {
        if (!File.Exists(_savePath))
        {
            return;
        }

        try
        {
            await using FileStream stream = File.OpenRead(_savePath);
            List<WelcomeScreenRecentItem>? loadedItems = await JsonSerializer.DeserializeAsync(
                stream, RecentItemJsonContext.Default.ListWelcomeScreenRecentItem);

            if (loadedItems == null)
            {
                return;
            }

            _recentItems.Clear();
            foreach (WelcomeScreenRecentItem item in loadedItems)
            {
                _recentItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            _logs.Info($"Failed to load recent items: {ex}");
            File.Delete(_savePath);
        }
    }

    public async Task SaveRecentItemsAsync()
    {
        List<WelcomeScreenRecentItem> itemsToSave = RecentItems.ToList();

        try
        {
            await using FileStream fs = File.Create(_savePath);
            await JsonSerializer.SerializeAsync(fs, itemsToSave,
                RecentItemJsonContext.Default.ListWelcomeScreenRecentItem);
        }
        catch (Exception ex)
        {
            _logs.Info($"Failed to save recent items: {ex}");
        }
    }
}

[JsonSerializable(typeof(WelcomeScreenRecentItem))]
[JsonSerializable(typeof(List<WelcomeScreenRecentItem>))]
[JsonSourceGenerationOptions(WriteIndented = true, UseStringEnumConverter = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
public partial class RecentItemJsonContext : JsonSerializerContext;