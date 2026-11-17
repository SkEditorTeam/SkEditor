using System;
using System.Collections.Frozen;
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
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Generators.Services;

namespace SkEditor.Editor.Generators.ViewModels;

public sealed partial class ItemPaletteViewModel : ObservableObject, IDisposable
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

    public BindableReactiveProperty<bool> FailedToLoad { get; } = new(false);
    public BindableReactiveProperty<bool> IsLoading { get; } = new(false);
    public BindableReactiveProperty<bool> IsLoaded { get; } = new(false);

    public BindableReactiveProperty<string> Filter { get; } = new(string.Empty);
    public NotifyCollectionChangedSynchronizedViewList<ItemViewModel> Items { get; }

    public BindableReactiveProperty<ItemViewModel?> SelectedItem { get; } = new(null);

    private readonly ObservableList<ItemViewModel> _sourceItems = [];
    private FrozenDictionary<string, ItemViewModel> _itemsById = FrozenDictionary<string, ItemViewModel>.Empty;

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
        
        FailedToLoad.AddTo(ref _disposables);
        IsLoading.AddTo(ref _disposables);
        IsLoaded.AddTo(ref _disposables);

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
        FailedToLoad.Value = false;
        IsLoading.Value = true;
        IsLoaded.Value = false;
        CancellationToken ct = _cts.Token;

        try
        {
            Item[]? rawItems = await _resources.GetItemsAsync(ct);

            if (rawItems is null)
            {
                bool success = await Download(ct);
                if (!success || ct.IsCancellationRequested)
                {
                    FailedToLoad.Value = true;
                    return;
                }

                rawItems = await _resources.GetItemsAsync(ct);
                if (rawItems is null)
                {
                    FailedToLoad.Value = true;
                    _notifs.Debug("Failed to load items after downloading resources.");
                    return;
                }
            }

            ClearItems();

            ItemViewModel[] items = new ItemViewModel[rawItems.Length];

            await Task.Run(() =>
            {
                ParallelOptions options = new() { CancellationToken = ct };
                Parallel.For(0, rawItems.Length, options, i =>
                {
                    Item item = rawItems[i];
                    using MemoryStream stream = new(item.Texture);
                    Bitmap bitmap = Bitmap.DecodeToWidth(stream, 64);

                    ItemViewModel itemVm = new(item.Id, item.DisplayName)
                    {
                        Image = bitmap
                    };

                    items[i] = itemVm;
                });

                Array.Sort(items, ByNameComparer);

                _itemsById = items.ToFrozenDictionary(
                    x => x.Id,
                    StringComparer.OrdinalIgnoreCase);
            }, ct);

            if (ct.IsCancellationRequested)
            {
                return;
            }

            _sourceItems.AddRange(items);
            SelectedItem.Value = Items.FirstOrDefault();
            IsLoaded.Value = true;
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
        finally
        {
            IsLoading.Value = false;
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
            Notification notification = Notification
                .Create(TranslationKey.GuiGeneratorResourcesDownloadFailedTitle)
                .WithMessage(TranslationKey.GuiGeneratorResourcesDownloadFailedMessage)
                .WithType(NotificationType.Error);

            _notifs.Show(notification);
            _logs.Error(ex, "Failed to download resources");
            return false;
        }
    }

    public ItemViewModel? FindById(string id) => _itemsById.GetValueOrDefault(id);

    private void ClearItems()
    {
        foreach (ItemViewModel item in _sourceItems)
        {
            item.Dispose();
        }

        _sourceItems.Clear();
        _itemsById = FrozenDictionary<string, ItemViewModel>.Empty;
    }

    public void Dispose()
    {
        _cts.Cancel();
        ClearItems();
        _cts.Dispose();
        _disposables.Dispose();
        SelectedItem.Dispose();
    }
}