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

public sealed partial class GuiProjectsViewModel : ObservableObject, IDisposable
{
    private readonly IGuiProjectService _projects;
    private readonly IDialogService _dialogs;
    private readonly ISynchronizedView<GuiProject, GuiProjectViewModel> _projectsView;
    private readonly IDisposable _activeSub;

    public GuiProjectsViewModel(IGuiProjectService projects, IDialogService dialogs)
    {
        _projects = projects;
        _dialogs = dialogs;
        
        _projectsView = projects.Projects.CreateView(p =>
            new GuiProjectViewModel(p, p.Id == projects.ActiveProject.CurrentValue?.Id));
        
        Projects = _projectsView.ToNotifyCollectionChanged();
        
        _activeSub = projects.ActiveProject.Subscribe(active =>
        {
            foreach (GuiProjectViewModel view in _projectsView)
            {
                view.IsActive = view.Model.Id == active?.Id;
            }
        });
    }

    public NotifyCollectionChangedSynchronizedViewList<GuiProjectViewModel> Projects { get; }

    [ObservableProperty]
    public partial GuiProjectViewModel? SelectedProject { get; set; }

    public async Task LoadProjects()
    {
        await _projects.LoadProjects();
    }
    
    public async Task SaveProject(GuiBlueprint blueprint)
    {
        GuiProject project;

        if (_projects.ActiveProject.CurrentValue is { } activeProject)
        {
            activeProject.Blueprint = blueprint;
            project = activeProject;
        }
        else
        {
            string? promptName = await PromptName("Save project".ToTranslatable(), "New project".ToTranslatable());

            if (promptName is null)
            {
                return;
            }

            string name = !string.IsNullOrWhiteSpace(promptName)
                ? promptName.Trim()
                : "New project";

            project = new GuiProject
            {
                Name = name,
                Blueprint = blueprint
            };
        }

        await _projects.SaveProject(project);
    }

    public void NewProject(BindableReactiveProperty<string> title, BindableReactiveProperty<string> functionName, GuiGridViewModel grid)
    {
        _projects.SetActiveProject(null);

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

        if (SelectedProject.Model == _projects.ActiveProject.CurrentValue)
        {
            NewProject(title, functionName, grid);
        }

        _projects.DeleteProject(SelectedProject.Model);
    }

    public async Task RenameProject()
    {
        if (SelectedProject is null)
        {
            return;
        }

        string? promptName = await PromptName("Rename project".ToTranslatable(), "New name".ToTranslatable(), SelectedProject.Name);
        string? name = promptName?.Trim();

        if (string.IsNullOrWhiteSpace(name) || name == SelectedProject.Name)
        {
            return;
        }

        await _projects.RenameProject(SelectedProject.Model, name);
    }

    private async Task<string?> PromptName(TranslatableString title, TranslatableString placeholder, string? initialValue = null)
    {
        TextBoxDialogElement inputElement = new() { Text = initialValue ?? string.Empty, PlaceholderText = placeholder };

        Dialog dialog = Dialog
            .Create(title)
            .WithIcon(Symbol.SaveEdit)
            .WithElements(inputElement)
            .WithButtons(
                new DialogButton(TranslationKey.ButtonRename, DialogResult.Yes, true),
                DialogButton.Cancel
            );

        DialogResult result = await _dialogs.ShowDialogAsync(dialog);

        return result == StandardDialogResult.Yes
            ? inputElement.Text
            : null;
    }

    public void LoadProject(BindableReactiveProperty<string> title, BindableReactiveProperty<string> functionName, GuiGridViewModel grid, ItemPaletteViewModel items)
    {
        if (SelectedProject is null)
        {
            return;
        }

        _projects.SetActiveProject(SelectedProject.Model);

        GuiBlueprint blueprint = SelectedProject.Model.Blueprint;

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

    public void Dispose()
    {
        _projectsView.Dispose();
        _activeSub.Dispose();
        Projects.Dispose();
    }
}