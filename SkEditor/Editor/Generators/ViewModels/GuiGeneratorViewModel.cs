using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using ObservableCollections;
using R3;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Dialogs.Models.Elements;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Generators.Extensions;
using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Generators.Projects;
using SkEditor.Editor.Generators.Providers;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Tabs;

namespace SkEditor.Editor.Generators.ViewModels;

public partial class GuiGeneratorViewModel : ObservableObject, IDisposable
{
    private readonly ITabService _tabs;
    private readonly IGuiProjectService _projects;
    private readonly IDialogService _dialogs;
    private bool _disposed;

    public BindableReactiveProperty<GuiGeneratorTab> CurrentViewMode { get; } = new(GuiGeneratorTab.Palette);
    public BindableReactiveProperty<string> FunctionName { get; } = new(string.Empty);
    public GuiGridViewModel Grid { get; } = new();

    public ItemPaletteViewModel ItemPalette { get; }
    public IReadOnlyList<IGuiCodeProvider> Providers { get; }

    public NotifyCollectionChangedSynchronizedViewList<GuiProject> Projects { get; }

    [ObservableProperty]
    public partial GuiProject? SelectedProject { get; set; }

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
        _projects = projects;
        _tabs = tabs;
        _dialogs = dialogs;
        ItemPalette = new ItemPaletteViewModel(resources, notifs, logs);
        Providers = [.. providers];
        SelectedProvider = Providers.Count > 0 ? Providers[0] : null;

        Projects = projects.Projects.ToNotifyCollectionChangedSlim();
    }

    public async Task LoadProjects()
    {
        await _projects.LoadProjects();
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

        GuiBlueprint blueprint = CreateBlueprint();
        string code = SelectedProvider.Generate(blueprint);
        if (_tabs.SelectedTab is ITextEditorTab editorTab)
        {
            editorTab.InsertCode(code);
        }
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        GuiBlueprint blueprint = CreateBlueprint();
        
        GuiProject project;

        if (_projects.ActiveProject is { } activeProject)
        {
            activeProject.Blueprint = blueprint;
            project = activeProject;
        }
        else
        {
            TextBoxDialogElement inputElement = new() { PlaceholderText = "New project".ToTranslatable() };

            Dialog dialog = Dialog
                .Create("Save project".ToTranslatable())
                .WithIcon(Symbol.Save)
                .WithElements(inputElement)
                .WithButtons(
                    new DialogButton(TranslationKey.ButtonSave, DialogResult.Yes, true),
                    DialogButton.Cancel
                );

            DialogResult result = await _dialogs.ShowDialogAsync(dialog);

            if (result != StandardDialogResult.Yes)
            {
                return;
            }

            string title = !string.IsNullOrWhiteSpace(inputElement.Text) 
                ? inputElement.Text.Trim() 
                : "New project";
            
            project = new GuiProject
            {
                Name = title,
                Blueprint = blueprint
            };
        }

        await _projects.SaveProject(project);
    }

    [RelayCommand]
    private void NewProject()
    {
        _projects.SetActiveProject(null);

        Title.Value = string.Empty;
        FunctionName.Value = string.Empty;
        Grid.Rows.Value = 5;
        Grid.ClearAllSlots();
        Grid.ClearBackground();
    }
    
    [RelayCommand]
    private void DeleteProject()
    {
        if (SelectedProject is null)
        {
            return;
        }

        if (SelectedProject == _projects.ActiveProject)
        {
            NewProject();
        }
        
        _projects.DeleteProject(SelectedProject);
    }

    [RelayCommand]
    private void LoadProject()
    {
        if (SelectedProject is null)
        {
            return;
        }

        _projects.SetActiveProject(SelectedProject);

        GuiBlueprint blueprint = SelectedProject.Blueprint;

        Title.Value = blueprint.Title ?? string.Empty;
        FunctionName.Value = blueprint.FunctionName ?? string.Empty;
        
        Grid.ClearAllSlots();
        Grid.Rows.Value = Math.Clamp(blueprint.Rows, 1, 6);
        
        if (blueprint.BackgroundItem is { } backgroundItem)
        {
            ItemViewModel? bgVm = ItemPalette.FindById(backgroundItem.Id);
            Grid.BackgroundItem = GuiItemViewModel.FromModel(backgroundItem, bgVm);
        }
        
        foreach ((int slot, GuiItem item) in blueprint.Items)
        {
            if (slot < 0 || slot >= Grid.Slots.Count)
            {
                continue;
            }
            
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
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        GC.SuppressFinalize(this);
        ItemPalette.Dispose();
        Grid.Dispose();
    }
}