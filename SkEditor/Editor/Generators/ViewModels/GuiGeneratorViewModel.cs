using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Generators.Extensions;
using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Generators.Providers;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs;

namespace SkEditor.Editor.Generators.ViewModels;

public partial class GuiGeneratorViewModel : ObservableObject, IDisposable
{
    private readonly ITabService _tabs;
    private DisposableBag _disposables;
    private bool _disposed;

    public BindableReactiveProperty<GuiGeneratorTab> CurrentViewMode { get; } = new(GuiGeneratorTab.Palette);
    public BindableReactiveProperty<string> FunctionName { get; } = new(string.Empty);
    public GuiGridViewModel Grid { get; } = new();

    public ItemPaletteViewModel ItemPalette { get; }

    public IReadOnlyList<IGuiCodeProvider> Providers { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    public partial IGuiCodeProvider? SelectedProvider { get; set; }

    public BindableReactiveProperty<string> Title { get; } = new(string.Empty);

    private bool CanGenerate => SelectedProvider is not null;

    public GuiGeneratorViewModel(IGameResourcesService resources,
        INotificationService notifs,
        ILogService logs,
        ITabService tabs,
        IEnumerable<IGuiCodeProvider> providers)
    {
        _tabs = tabs;
        ItemPalette = new ItemPaletteViewModel(resources, notifs, logs);
        Providers = [.. providers];
        SelectedProvider = Providers.Count > 0 ? Providers[0] : null;
    }

    [RelayCommand]
    private void SelectSlot(SlotViewModel slot)
    {
        Grid.SelectSlot(slot);
    }

    [RelayCommand]
    private void ApplySelectedItem()
    {
        Grid.ApplySelectedItem(ItemPalette.SelectedItem.Value);
    }

    [RelayCommand]
    private void SetAllEmptySlots()
    {
        Grid.SetAllEmptySlots(ItemPalette.SelectedItem.Value);
    }

    [RelayCommand]
    private void FillBackground()
    {
        Grid.FillBackground(ItemPalette.SelectedItem.Value);
    }

    [RelayCommand]
    private void ClearSlot()
    {
        Grid.ClearSlot();
    }

    [RelayCommand]
    private void ClearAllSlots()
    {
        Grid.ClearAllSlots();
    }

    [RelayCommand]
    private void ClearBackground()
    {
        Grid.ClearBackground();
    }

    [RelayCommand]
    private void HandleDoubleClick(SlotViewModel slot)
    {
        Grid.SelectSlot(slot);
        if (slot.Item is null)
        {
            Grid.ApplySelectedItem(ItemPalette.SelectedItem.Value);
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

        switch (action.ClickType)
        {
            case ClickType.Left:
                SelectSlot(slot);
                break;

            case ClickType.Middle:
                Grid.SelectSlot(slot);
                Grid.ApplySelectedItem(ItemPalette.SelectedItem.Value);
                break;
        }
    }

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private void Generate()
    {
        if (SelectedProvider is null)
        {
            throw new InvalidOperationException("Selected provider is null");
        }

        Dictionary<int, GuiItem> slots = new();
        foreach (SlotViewModel slot in Grid.Slots)
        {
            if (slot.Item is not null && GuiItem.FromViewModel(slot.Item) is { } guiItem)
            {
                slots.Add(slot.Index, guiItem);
            }
        }

        GuiBlueprint blueprint = new()
        {
            Title = !string.IsNullOrEmpty(Title.Value) ? Title.Value : null,
            FunctionName = !string.IsNullOrEmpty(FunctionName.Value) ? FunctionName.Value : null,
            Rows = Math.Clamp(Grid.Rows.Value, 1, 6),
            BackgroundItem = GuiItem.FromViewModel(Grid.BackgroundItem),
            Items = slots
        };

        string code = SelectedProvider.Generate(blueprint);
        if (_tabs.SelectedTab is ITextEditorTab editorTab)
        {
            editorTab.InsertCode(code);
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