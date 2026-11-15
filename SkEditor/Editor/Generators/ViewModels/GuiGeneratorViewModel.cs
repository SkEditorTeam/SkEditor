using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using R3;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Generators.Extensions;
using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Generators.Providers;
using SkEditor.Editor.Generators.Services;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs;

namespace SkEditor.Editor.Generators.ViewModels;

public sealed partial class GuiGeneratorViewModel : ObservableObject, IDisposable
{
    private readonly ITabService _tabs;
    private DisposableBag _disposables;

    public BindableReactiveProperty<GuiGeneratorTab> CurrentViewMode { get; } = new(GuiGeneratorTab.Palette);
    public BindableReactiveProperty<string> FunctionName { get; } = new(string.Empty);

    public GuiGridViewModel Grid { get; }
    public ItemPaletteViewModel ItemPalette { get; }
    public GuiProjectsViewModel Projects { get; }

    public IReadOnlyList<IGuiCodeProvider> Providers { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateCommand))]
    public partial IGuiCodeProvider? SelectedProvider { get; set; }

    public BindableReactiveProperty<string> Title { get; } = new(string.Empty);

    private bool CanGenerate => SelectedProvider is not null;

    public GuiGeneratorViewModel(IGameResourcesService resources,
        IGuiProjectService projects,
        INotificationService notifs,
        IDialogService dialogs,
        ILogService logs,
        ITabService tabs,
        IEnumerable<IGuiCodeProvider> providers)
    {
        _tabs = tabs;

        ItemPalette = new ItemPaletteViewModel(resources, notifs, logs).AddTo(ref _disposables);
        Grid = new GuiGridViewModel(ItemPalette.SelectedItem).AddTo(ref _disposables);
        Projects = new GuiProjectsViewModel(projects, dialogs, CreateBlueprint).AddTo(ref _disposables);
        Projects.BlueprintLoaded.Subscribe(ApplyBlueprint).AddTo(ref _disposables);

        Providers = [.. providers];
        SelectedProvider = Providers.Count > 0 ? Providers[0] : null;
    }

    [RelayCommand(CanExecute = nameof(CanGenerate))]
    private void Generate()
    {
        if (SelectedProvider is null)
        {
            throw new InvalidOperationException("Selected provider is null");
        }

        GuiBlueprint blueprint = CreateBlueprint();
        string code = SelectedProvider.Generate(blueprint);
        if (_tabs.SelectedTab is ITextEditorTab editorTab)
        {
            editorTab.InsertCode(code);
        }
    }
    
    private void ApplyBlueprint(GuiBlueprint? blueprint)
    {
        Title.Value = blueprint?.Title ?? string.Empty;
        FunctionName.Value = blueprint?.FunctionName ?? string.Empty;

        Grid.ClearAllSlots();
        Grid.ClearBackground();

        if (blueprint is null)
        {
            Grid.Rows.Value = 5;
            return;
        }

        Grid.Rows.Value = Math.Clamp(blueprint.Rows, 1, 6);

        if (blueprint.BackgroundItem is { } backgroundItem)
        {
            ItemViewModel? bgVm = ItemPalette.FindById(backgroundItem.Id);
            Grid.BackgroundItem = GuiItemViewModel.FromModel(backgroundItem, bgVm);
        }

        foreach ((int slot, GuiItem item) in blueprint.Items)
        {
            if (slot < 0 || slot >= Grid.Slots.Count) continue;

            ItemViewModel? itemVm = ItemPalette.FindById(item.Id);
            Grid.Slots[slot].Item = GuiItemViewModel.FromModel(item, itemVm);
        }
    }

    private GuiBlueprint CreateBlueprint()
    {
        Dictionary<int, GuiItem> slots = new();
        foreach (SlotViewModel slot in Grid.Slots)
        {
            if (slot.Item is not null && GuiItem.FromViewModel(slot.Item) is { } guiItem)
            {
                slots.Add(slot.Index, guiItem);
            }
        }

        return new GuiBlueprint
        {
            Title = !string.IsNullOrEmpty(Title.Value) ? Title.Value : null,
            FunctionName = !string.IsNullOrEmpty(FunctionName.Value) ? FunctionName.Value : null,
            Rows = Math.Clamp(Grid.Rows.Value, 1, 6),
            BackgroundItem = GuiItem.FromViewModel(Grid.BackgroundItem),
            Items = slots
        };
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}