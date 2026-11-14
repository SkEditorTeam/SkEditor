using System;
using System.Collections.Generic;
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

namespace SkEditor.Editor.Generators.ViewModels;

public partial class ItemPaletteViewModel : ObservableObject, IDisposable
{
    private static readonly Comparer<ItemViewModel> ByNameComparer = Comparer<ItemViewModel>.Create((a, b) =>
    {
        if (ReferenceEquals(a, b))
        {
            return 0;
        }

        return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
    });

    private static readonly Comparer<ItemViewModel> ByScoreComparer =
        Comparer<ItemViewModel>.Create((a, b) => b.MatchScore.CompareTo(a.MatchScore));

    private static readonly Comparer<ItemViewModel> CombinedComparer = Comparer<ItemViewModel>.Create((a, b) =>
    {
        if (ReferenceEquals(a, b))
        {
            return 0;
        }

        int scoreResult = ByScoreComparer.Compare(a, b);
        return scoreResult != 0 ? scoreResult : ByNameComparer.Compare(a, b);
    });
    
    public BindableReactiveProperty<string> Filter { get; } = new(string.Empty);
    public NotifyCollectionChangedSynchronizedViewList<ItemViewModel> Items { get; }

    public BindableReactiveProperty<ItemViewModel?> SelectedItem { get; } = new(null);

    private readonly ObservableList<ItemViewModel> _sourceItems = [];
    private DisposableBag _disposables;
    
    private readonly CancellationTokenSource _cts = new();
    private readonly ILogService _logs;
    private readonly INotificationService _notifs;
    private readonly IGameResourcesService _resources;
    
    public ItemPaletteViewModel(IGameResourcesService resources, INotificationService notifs, ILogService logs)
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
                        item.MatchScore = ItemSearchEngine.GetMatchScore(item.Name, query);
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
        CancellationToken ct = _cts.Token;

        try
        {
            Item[]? rawItems = await _resources.GetItemsAsync(ct);

            if (rawItems is null)
            {
                bool success = await Download(ct);
                if (!success || ct.IsCancellationRequested)
                {
                    return;
                }

                rawItems = await _resources.GetItemsAsync(ct);
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
                Parallel.For(0, rawItems.Length, (i, state) =>
                {
                    if (ct.IsCancellationRequested)
                    {
                        state.Stop();
                    }

                    Item item = rawItems[i];
                    using MemoryStream stream = new(item.Texture);
                    Bitmap bitmap = Bitmap.DecodeToWidth(stream, 64);

                    items[i] = new ItemViewModel(item.Id, item.DisplayName)
                    {
                        Image = bitmap
                    };
                });

                Array.Sort(items, ByNameComparer);
            }, ct);

            if (ct.IsCancellationRequested)
            {
                return;
            }

            _sourceItems.AddRange(items);
            SelectedItem.Value = Items.FirstOrDefault();
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }
    
    private async Task<bool> Download(CancellationToken ct)
    {
        try
        {
            bool success = await _resources.DownloadResources(ct);
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
        _cts.Cancel();
        _cts.Dispose();
        _disposables.Dispose();
        SelectedItem.Dispose();
    }
}