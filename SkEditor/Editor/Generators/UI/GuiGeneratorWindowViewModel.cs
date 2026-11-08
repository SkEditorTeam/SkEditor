using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ObservableCollections;
using R3;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators.UI;

public partial class GuiGeneratorWindowViewModel : ObservableObject, IDisposable
{
    private static readonly Comparer<ItemViewModel> ByNameComparer = Comparer<ItemViewModel>.Create((a, b) =>
        string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

    private static readonly Comparer<ItemViewModel> ByScoreComparer =
        Comparer<ItemViewModel>.Create((a, b) => b.MatchScore.CompareTo(a.MatchScore));
    
    private static readonly Comparer<ItemViewModel> CombinedComparer = Comparer<ItemViewModel>.Create((a, b) =>
    {
        int scoreResult = ByScoreComparer.Compare(a, b);

        return scoreResult != 0
            ? scoreResult
            : ByNameComparer.Compare(a, b);
    });


    private readonly ILogService _logs;
    private readonly INotificationService _notifs;
    private readonly IGameResourcesService _resources;
    private readonly ObservableList<ItemViewModel> _sourceItems = [];
    private DisposableBag _disposables;

    public BindableReactiveProperty<string> Filter { get; } = new(string.Empty);

    public NotifyCollectionChangedSynchronizedViewList<ItemViewModel> Items { get; }
    public BindableReactiveProperty<ItemViewModel?> SelectedItem { get; } = new(null);

    public GuiGeneratorWindowViewModel(IGameResourcesService resources, INotificationService notifs, ILogService logs)
    {
        _resources = resources;
        _notifs = notifs;
        _logs = logs;

        ISynchronizedView<ItemViewModel, ItemViewModel> view = _sourceItems.CreateView(x => x).AddTo(ref _disposables);
        Items = view.ToNotifyCollectionChanged().AddTo(ref _disposables);

        Filter
            .ThrottleLast(TimeSpan.FromMilliseconds(40))
            .Subscribe(query =>
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    view.ResetFilter();
                    _sourceItems.Sort(ByNameComparer);
                }
                else
                {
                    foreach (ItemViewModel item in _sourceItems)
                    {
                        item.MatchScore = ItemSearchEngine.GetMatchScore(item.DisplayName, query);
                    }

                    _sourceItems.Sort(CombinedComparer);

                    view.AttachFilter(item => item.MatchScore > 0);
                }

                SelectedItem.Value = Items.FirstOrDefault();
            })
            .AddTo(ref _disposables);
    }

    [RelayCommand]
    private async Task Load()
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        Item[]? rawItems = await _resources.GetItemsAsync(CancellationToken.None);

        if (rawItems is null)
        {
            bool success = await Download();
            if (!success)
            {
                return;
            }

            rawItems = await _resources.GetItemsAsync(CancellationToken.None);
            if (rawItems is null)
            {
                _notifs.Debug("Failed to load items after downloading resources.");
                return;
            }
        }

        ClearItems();

        ItemViewModel[] items = new ItemViewModel[rawItems.Length];

        await Task.Run(() =>
        {
            Parallel.For(0, rawItems.Length, i =>
            {
                Item item = rawItems[i];
                using MemoryStream stream = new(item.Texture);
                Bitmap bitmap = Bitmap.DecodeToWidth(stream, 32);

                items[i] = new ItemViewModel(item.Id, item.DisplayName)
                {
                    Image = bitmap
                };
            });
            
            Array.Sort(items, ByNameComparer);
        });
        
        _sourceItems.AddRange(items);
        SelectedItem.Value = Items.FirstOrDefault();

        stopwatch.Stop();
        _notifs.Debug($"Loaded {Items.Count} items in {stopwatch.ElapsedMilliseconds} ms");
    }

    private async Task<bool> Download()
    {
        try
        {
            bool success = await _resources.DownloadResources(CancellationToken.None);
            return success;
        }
        catch (Exception ex)
        {
            _notifs.Debug($"Something went wrong: {ex.Message}");
            _logs.Error(ex, "Failed to download resources");
            return false;
        }
    }

    private void ClearItems()
    {
        foreach (ItemViewModel item in _sourceItems)
        {
            item.Dispose();
        }

        _sourceItems.Clear();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);

        ClearItems();
        _disposables.Dispose();
        Items.Dispose();
        SelectedItem.Dispose();
        Filter.Dispose();
    }
}

public class ItemViewModel(string id, string displayName) : ObservableObject, IDisposable
{
    public string DisplayName { get; } = displayName;
    public string Id { get; } = id;
    public required Bitmap Image { get; init; }

    internal int MatchScore { get; set; }

    public override string ToString()
    {
        return DisplayName;
    }

    public void Dispose()
    {
        Image?.Dispose();
    }
}