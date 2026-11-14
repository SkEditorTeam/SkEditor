using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Input;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs;

namespace SkEditor.Editor.Generators.ViewModels;

public partial class GuiGeneratorWindowViewModel : ObservableObject, IDisposable
{
    private readonly ITabService _tabs;

    private DisposableBag _disposables;
    private bool _disposed;
    public static int Columns => 9;

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

    public ItemPaletteViewModel ItemPalette { get; }

    public GuiGeneratorWindowViewModel(IGameResourcesService resources, INotificationService notifs, ILogService logs, ITabService tabs)
    {
        _tabs = tabs;
        ItemPalette = new ItemPaletteViewModel(resources, notifs, logs);
        
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

        ItemViewModel? selected = ItemPalette.SelectedItem.Value;

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
        if (ItemPalette.SelectedItem.Value is null)
        {
            return;
        }

        foreach (SlotViewModel slot in Slots)
        {
            slot.Item ??= new GuiItemViewModel(ItemPalette.SelectedItem.Value.Id, ItemPalette.SelectedItem.Value.Name,
                ItemPalette.SelectedItem.Value.Image);
        }
    }
    
    [RelayCommand]
    private void FillBackground()
    {
        if (ItemPalette.SelectedItem.Value is null)
        {
            return;
        }

        BackgroundItem = new GuiItemViewModel(ItemPalette.SelectedItem.Value.Id, ItemPalette.SelectedItem.Value.Name,
            ItemPalette.SelectedItem.Value.Image)
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

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
        _disposables.Dispose();
    }
}