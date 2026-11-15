using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentIcons.Common;
using ObservableCollections;
using R3;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Dialogs.Models.Elements;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;
using SkEditor.Editor.Generators.Extensions;
using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Generators.ViewModels;

namespace SkEditor.Editor.Generators.Projects;

public partial class GuiProjectsViewModel(IGuiProjectService projects, IDialogService dialogs) : ObservableObject
{
    public NotifyCollectionChangedSynchronizedViewList<GuiProject> Projects { get; } =
        projects.Projects.ToNotifyCollectionChangedSlim();

    [ObservableProperty]
    public partial GuiProject? SelectedProject { get; set; }

    public async Task LoadProjects()
    {
        await projects.LoadProjects();
    }
    
    public async Task SaveProject(GuiBlueprint blueprint)
    {
        GuiProject project;

        if (projects.ActiveProject is { } activeProject)
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

            DialogResult result = await dialogs.ShowDialogAsync(dialog);

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

        await projects.SaveProject(project);
    }

    public void NewProject(BindableReactiveProperty<string> title, BindableReactiveProperty<string> functionName, GuiGridViewModel grid)
    {
        projects.SetActiveProject(null);

        title.Value = string.Empty;
        functionName.Value = string.Empty;
        grid.Rows.Value = 5;
        grid.ClearAllSlots();
        grid.ClearBackground();
    }

    public void DeleteProject(BindableReactiveProperty<string> title, BindableReactiveProperty<string> functionName, GuiGridViewModel grid)
    {
        if (SelectedProject is null)
        {
            return;
        }

        if (SelectedProject == projects.ActiveProject)
        {
            NewProject(title, functionName, grid);
        }

        projects.DeleteProject(SelectedProject);
    }

    public void LoadProject(BindableReactiveProperty<string> title, BindableReactiveProperty<string> functionName, GuiGridViewModel grid, ItemPaletteViewModel items)
    {
        if (SelectedProject is null)
        {
            return;
        }

        projects.SetActiveProject(SelectedProject);

        GuiBlueprint blueprint = SelectedProject.Blueprint;

        title.Value = blueprint.Title ?? string.Empty;
        functionName.Value = blueprint.FunctionName ?? string.Empty;

        grid.ClearAllSlots();
        grid.Rows.Value = Math.Clamp(blueprint.Rows, 1, 6);

        if (blueprint.BackgroundItem is { } backgroundItem)
        {
            ItemViewModel? bgVm = items.FindById(backgroundItem.Id);
            grid.BackgroundItem = GuiItemViewModel.FromModel(backgroundItem, bgVm);
        }

        foreach ((int slot, GuiItem item) in blueprint.Items)
        {
            if (slot < 0 || slot >= grid.Slots.Count)
            {
                continue;
            }

            ItemViewModel? itemVm = items.FindById(item.Id);
            grid.Slots[slot].Item = GuiItemViewModel.FromModel(item, itemVm);
        }
    }
}