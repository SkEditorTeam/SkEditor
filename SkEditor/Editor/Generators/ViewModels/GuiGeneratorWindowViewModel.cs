using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
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

    private readonly CancellationTokenSource _cts = new();

    private readonly ILogService _logs;
    private readonly INotificationService _notifs;
    private readonly IGameResourcesService _resources;

    private readonly ObservableList<ItemViewModel> _sourceItems = [];
    private DisposableBag _disposables;
    private bool _disposed;
    public static int Columns => 9;

    public BindableReactiveProperty<string> Filter { get; } = new(string.Empty);
    public NotifyCollectionChangedSynchronizedViewList<ItemViewModel> Items { get; }

    [ObservableProperty]
    public partial int Rows { get; set; } = 5;

    public BindableReactiveProperty<ItemViewModel?> SelectedItem { get; } = new(null);

    [ObservableProperty]
    public partial SlotViewModel? SelectedSlot { get; set; }

    public ObservableCollection<SlotViewModel> Slots { get; } = [];
    
    public BindableReactiveProperty<GuiGeneratorViewMode> CurrentViewMode { get; } = new(GuiGeneratorViewMode.Palette);

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
                        item.MatchScore = ItemSearchEngine.GetMatchScore(item.Name, query);
                    }

                    _sourceItems.Sort(CombinedComparer);
                    view.AttachFilter(item => item.MatchScore > 0);
                }

                SelectedItem.Value = Items.FirstOrDefault();
            })
            .AddTo(ref _disposables);

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

    [RelayCommand]
    private void ApplySelectedItem()
    {
        if (SelectedSlot is null)
        {
            return;
        }

        ItemViewModel? selected = SelectedItem.Value;

        if (selected is null)
        {
            SelectedSlot.Item = null;
            return;
        }

        SelectedSlot.Item = new GuiItemViewModel(selected.Id, selected.Name, selected.Image);
    }

    [RelayCommand]
    private void SetAllEmptySlots()
    {
        if (SelectedItem.Value is null)
        {
            return;
        }

        foreach (SlotViewModel slot in Slots)
        {
            slot.Item ??= new GuiItemViewModel(SelectedItem.Value.Id, SelectedItem.Value.Name,
                SelectedItem.Value.Image);
        }
    }
    
    [RelayCommand]
    private void FillBackground()
    {
        if (SelectedItem.Value is null)
        {
            return;
        }

        foreach (SlotViewModel slot in Slots)
        {
            if (slot.Item is null || slot.Item.IsBackground)
            {
                slot.Item = new GuiItemViewModel(SelectedItem.Value.Id, SelectedItem.Value.Name,
                    SelectedItem.Value.Image)
                {
                    IsBackground = true
                };
            }
        }
    }

    [RelayCommand]
    private void ClearSlot()
    {
        SelectedSlot?.Item = null;
    }

    [RelayCommand]
    private void ClearAllSlots()
    {
        foreach (SlotViewModel slot in Slots)
        {
            slot.Item = null;
        }
    }

    [RelayCommand]
    private void ClearBackground()
    {
        foreach (SlotViewModel slot in Slots)
        {
            if (slot.Item?.IsBackground == true)
            {
                slot.Item = null;
            }
        }
    }

    [RelayCommand]
    private void HandleDoubleClick(SlotViewModel slot)
    {
        SelectedSlot = slot;
        if (SelectedSlot.Item is null)
        {
            ApplySelectedItem();
        }
        else
        {
            ClearSlot();
        }
    }

    [RelayCommand]
    private void SlotPointerPressed(SlotPointerAction? action)
    {
        if (action is null)
        {
            return;
        }

        SlotViewModel slot = action.Slot;

        switch (action.PointerKind)
        {
            case PointerUpdateKind.LeftButtonPressed:
                SelectSlot(slot);
                break;

            case PointerUpdateKind.MiddleButtonPressed:
                SelectSlot(slot);
                ApplySelectedItem();
                break;
        }
    }

    partial void OnRowsChanged(int value)
    {
        RecreateSlots();
    }

    private void RecreateSlots()
    {
        int targetCount = Rows * Columns;

        while (Slots.Count > targetCount)
        {
            Slots.RemoveAt(Slots.Count - 1);
        }

        while (Slots.Count < targetCount)
        {
            Slots.Add(new SlotViewModel(Slots.Count));
        }
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
        if (_disposed)
        {
            return;
        }

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