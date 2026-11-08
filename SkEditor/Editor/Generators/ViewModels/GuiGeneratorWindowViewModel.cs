using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

public partial class GuiGeneratorWindowViewModel : ObservableObject, IDisposable
{
    private static readonly Comparer<ItemViewModel> ByNameComparer = Comparer<ItemViewModel>.Create((a, b) =>
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a.IsNone) return -1;
        if (b.IsNone) return 1;

        return string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase);
    });

    private static readonly Comparer<ItemViewModel> ByScoreComparer =
        Comparer<ItemViewModel>.Create((a, b) => b.MatchScore.CompareTo(a.MatchScore));
    
    private static readonly Comparer<ItemViewModel> CombinedComparer = Comparer<ItemViewModel>.Create((a, b) =>
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a.IsNone) return -1;
        if (b.IsNone) return 1;

        int scoreResult = ByScoreComparer.Compare(a, b);
        return scoreResult != 0 ? scoreResult : ByNameComparer.Compare(a, b);
    });

    private readonly ILogService _logs;
    private readonly INotificationService _notifs;
    private readonly IGameResourcesService _resources;
    private readonly ObservableList<ItemViewModel> _sourceItems = [];
    private DisposableBag _disposables;
    private readonly CancellationTokenSource _cts = new();
    
    public BindableReactiveProperty<string> Filter { get; } = new(string.Empty);

    public NotifyCollectionChangedSynchronizedViewList<ItemViewModel> Items { get; }
    public BindableReactiveProperty<ItemViewModel?> SelectedItem { get; } = new(null);

    [ObservableProperty]
    public partial int Rows { get; set; } = 5;
    public static int Columns => 9;
    
    public ObservableCollection<SlotViewModel> Slots { get; } = [];

    [ObservableProperty]
    public partial SlotViewModel? SelectedSlot { get; set; }

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

                    view.AttachFilter(item => item.IsNone || item.MatchScore > 0);
                }

                SelectedItem.Value = Items.FirstOrDefault(i => !i.IsNone);
            })
            .AddTo(ref _disposables);

        SelectedItem.Subscribe(item =>
        {
            if (item is null) return;
            if (SelectedSlot is null) return;

            SelectedSlot.Item = new GuiItemViewModel(item.Id, item.DisplayName, item.Image);
        });
        
        RecreateSlots();
        if (Slots.FirstOrDefault() is { } firstSlot)
        {
            SelectSlot(firstSlot);
        }
    }
    
    [RelayCommand]
    private void SelectSlot(SlotViewModel slot)
    {
        SelectedSlot?.IsSelected = false;

        SelectedSlot = slot;
        slot.IsSelected = true;
    }

    private void RecreateSlots()
    {
        int targetCount = Rows * Columns;
        
        while (Slots.Count > targetCount)
            Slots.RemoveAt(Slots.Count - 1);

        while (Slots.Count < targetCount)
            Slots.Add(new SlotViewModel(Slots.Count));
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
                    if (ct.IsCancellationRequested) state.Stop();

                    Item item = rawItems[i];
                    using MemoryStream stream = new(item.Texture);
                    Bitmap bitmap = Bitmap.DecodeToWidth(stream, 32);

                    items[i] = new ItemViewModel(item.Id, item.DisplayName)
                    {
                        Image = bitmap
                    };
                });

                Array.Sort(items, ByNameComparer);
            }, ct);

            if (ct.IsCancellationRequested) return;

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
        _sourceItems.Add(ItemViewModel.None);
    }

    private bool _disposed;
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        GC.SuppressFinalize(this);
        
        _cts.Cancel();
        _cts.Dispose();

        ClearItems();
        _disposables.Dispose();
        Items.Dispose();
        SelectedItem.Dispose();
        Filter.Dispose();
    }
}