using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using R3;

namespace SkEditor.Editor.Generators.ViewModels;

public partial class GuiGridViewModel : ObservableObject, IDisposable
{
    private DisposableBag _disposables;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InspectedItem), nameof(IsEditingBackground))]
    public partial GuiItemViewModel? BackgroundItem { get; set; }

    public static int Columns => 9;

    public GuiItemViewModel? InspectedItem => SelectedSlot?.Item ?? BackgroundItem;
    public bool IsEditingBackground => SelectedSlot?.Item is null && BackgroundItem is not null;
    public BindableReactiveProperty<int> Rows { get; set; } = new(5);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(InspectedItem), nameof(IsEditingBackground))]
    public partial SlotViewModel? SelectedSlot { get; set; }

    public ObservableCollection<SlotViewModel> Slots { get; } = [];

    public GuiGridViewModel()
    {
        Rows.Subscribe(_ => RecreateSlots()).AddTo(ref _disposables);
        RecreateSlots();

        if (Slots.FirstOrDefault() is { } firstSlot)
        {
            SelectSlot(firstSlot);
        }

        SelectedSlot?
            .ObservePropertyChanged(x => nameof(x.Item))
            .Subscribe(_ =>
            {
                OnPropertyChanged(nameof(InspectedItem));
                OnPropertyChanged(nameof(IsEditingBackground));
            })
            .AddTo(ref _disposables);
    }

    public void SelectSlot(SlotViewModel slot)
    {
        SelectedSlot?.IsSelected = false;

        SelectedSlot = slot;
        slot.IsSelected = true;
    }

    public void ClearSlot()
    {
        SelectedSlot?.Item = null;
    }

    public void ClearAllSlots()
    {
        foreach (SlotViewModel slot in Slots)
        {
            slot.Item = null;
        }
    }

    public void ApplySelectedItem(ItemViewModel? item)
    {
        if (SelectedSlot is null)
        {
            return;
        }

        if (item is null)
        {
            SelectedSlot.Item = null;
            return;
        }

        SelectedSlot.Item = new GuiItemViewModel(item.Id, item.Name, item.Image);
    }

    public void SetAllEmptySlots(ItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        foreach (SlotViewModel slot in Slots)
        {
            slot.Item ??= new GuiItemViewModel(item.Id, item.Name,
                item.Image);
        }
    }

    public void FillBackground(ItemViewModel? item)
    {
        if (item is null)
        {
            return;
        }

        BackgroundItem = new GuiItemViewModel(item.Id, item.Name, item.Image)
        {
            DisplayName = " ",
            HideTooltip = true
        };
    }

    public void ClearBackground()
    {
        BackgroundItem = null;
    }

    public void RecreateSlots()
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

        if (SelectedSlot is not null && !Slots.Contains(SelectedSlot) && Slots.LastOrDefault() is {} lastSlot)
        {
            SelectSlot(lastSlot);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _disposables.Dispose();
    }
}