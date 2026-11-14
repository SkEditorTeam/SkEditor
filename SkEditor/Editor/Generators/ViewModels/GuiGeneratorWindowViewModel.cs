using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ObservableCollections;
using R3;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs;

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
    private readonly ITabService _tabs;

    private readonly ObservableList<ItemViewModel> _sourceItems = [];
    private DisposableBag _disposables;
    private bool _disposed;
    public static int Columns => 9;

    public BindableReactiveProperty<string> Filter { get; } = new(string.Empty);
    public NotifyCollectionChangedSynchronizedViewList<ItemViewModel> Items { get; }

    public BindableReactiveProperty<ItemViewModel?> SelectedItem { get; } = new(null);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InspectedItem), nameof(IsEditingBackground))]
    public partial SlotViewModel? SelectedSlot { get; set; }

    public ObservableCollection<SlotViewModel> Slots { get; } = [];
    
    public BindableReactiveProperty<GuiGeneratorViewMode> CurrentViewMode { get; } = new(GuiGeneratorViewMode.Palette);
    
    public BindableReactiveProperty<string> Title { get; set; } = new(string.Empty);
    public BindableReactiveProperty<string> FunctionName { get; set; } = new(string.Empty);
    public BindableReactiveProperty<int> Rows { get; set; } = new(5);
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InspectedItem), nameof(IsEditingBackground))]
    public partial GuiItemViewModel? BackgroundItem { get; set; }
    
    public GuiItemViewModel? InspectedItem => SelectedSlot?.Item ?? BackgroundItem;
    public bool IsEditingBackground => SelectedSlot?.Item is null && BackgroundItem is not null;

    public GuiGeneratorWindowViewModel(IGameResourcesService resources, INotificationService notifs, ILogService logs, ITabService tabs)
    {
        _resources = resources;
        _notifs = notifs;
        _logs = logs;
        _tabs = tabs;

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
        
        Rows.Subscribe(_ => RecreateSlots()).AddTo(ref _disposables);

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
        OnPropertyChanged(nameof(InspectedItem));
        OnPropertyChanged(nameof(IsEditingBackground));
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

        BackgroundItem = new GuiItemViewModel(SelectedItem.Value.Id, SelectedItem.Value.Name,
            SelectedItem.Value.Image)
        {
            DisplayName = " ",
            HideTooltip = true
        };
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
        BackgroundItem = null;
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

    [RelayCommand]
    private void Generate()
    {
        if (_tabs.SelectedTab is not ITextEditorTab textEditorTab)
        {
            return;
        }
        
        Dictionary<int, GuiItemViewModel> items = Slots
            .Where(s => s.Item is not null)
            .ToDictionary(slot => slot.Index, slot => slot.Item!);
        
        GuiCodeGenerator.Options options = new()
        {
            Title = Title.Value,
            FunctionName = FunctionName.Value,
            Rows = Rows.Value,
            BackgroundItem = BackgroundItem
        };

        string generatedCode = GuiCodeGenerator.Generate(items, options);

        int caretOffset = textEditorTab.CaretOffset;
        TextLocation location = textEditorTab.Document.GetLocation(caretOffset);
        DocumentLine? line = textEditorTab.Document.GetLineByNumber(location.Line);
        if (line is null)
        {
            return;
        }

        if (line.Length > 0)
        {
            textEditorTab.Document.Insert(line.EndOffset, "\n" + generatedCode);
        }
        else
        {
            textEditorTab.Document.Insert(caretOffset, generatedCode);
        }
    }

    private void RecreateSlots()
    {
        int targetCount = Rows.Value * Columns;

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