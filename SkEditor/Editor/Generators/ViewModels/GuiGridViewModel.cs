using System;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators.ViewModels;

public sealed partial class GuiGridViewModel : ObservableObject, IDisposable
{
    private DisposableBag _disposables;
    private readonly IReadOnlyBindableReactiveProperty<ItemViewModel?> _selectedPaletteItem;

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

    public GuiGridViewModel(IReadOnlyBindableReactiveProperty<ItemViewModel?> selectedPaletteItem)
    {
        _selectedPaletteItem = selectedPaletteItem;
        
        Rows.Subscribe(_ => RecreateSlots()).AddTo(ref _disposables);
        RecreateSlots();

        if (Slots.FirstOrDefault() is { } firstSlot)
        {
            SelectSlot(firstSlot);
        }

        this.
            ObservePropertyChanged(x => x.SelectedSlot, x => x.Item)
            .Subscribe(_ =>
            {
                OnPropertyChanged(nameof(InspectedItem));
                OnPropertyChanged(nameof(IsEditingBackground));
            })
            .AddTo(ref _disposables);
    }

    [RelayCommand]
    private void SelectSlot(SlotViewModel slot)
    {
        if (SelectedSlot == slot) return;
        
        SelectedSlot?.IsSelected = false;

        SelectedSlot = slot;
        slot.IsSelected = true;
    }

    [RelayCommand]
    private void ClearSlot()
    {
        SelectedSlot?.Item = null;
    }

    [RelayCommand]
    public void ClearAllSlots()
    {
        foreach (SlotViewModel slot in Slots)
        {
            slot.Item = null;
        }
    }

    [RelayCommand]
    private void ApplySelectedItem()
    {
        if (SelectedSlot is null)
        {
            return;
        }

        if (_selectedPaletteItem.Value is not {} item)
        {
            SelectedSlot.Item = null;
            return;
        }

        SelectedSlot.Item = new GuiItemViewModel(item.Id, item.Name, item.Image);
    }

    [RelayCommand]
    private void SetAllEmptySlots()
    {
        if (_selectedPaletteItem.Value is not { } item)
        {
            return;
        }

        foreach (SlotViewModel slot in Slots)
        {
            slot.Item ??= new GuiItemViewModel(item.Id, item.Name,
                item.Image);
        }
    }

    [RelayCommand]
    private void FillBackground()
    {
        if (_selectedPaletteItem.Value is not { } item)
        {
            return;
        }

        BackgroundItem = new GuiItemViewModel(item.Id, item.Name, item.Image)
        {
            HideTooltip = true
        };
    }

    [RelayCommand]
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
    
    [RelayCommand]
    private void SlotPointerPressed(SlotPointerAction? action)
    {
        if (action is null)
        {
            return;
        }

        SlotViewModel slot = action.Slot;

        switch (action.ClickType)
        {
            case ClickType.Left:
                SelectSlot(slot);
                break;

            case ClickType.Middle:
                SelectSlot(slot);
                ApplySelectedItem();
                break;
        }
    }
    
    [RelayCommand]
    private void HandleDoubleClick(SlotViewModel slot)
    {
        SelectSlot(slot);
        if (slot.Item is null)
        {
            ApplySelectedItem();
        }
        else
        {
            ClearSlot();
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}