using System;
using System.Linq;
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
using SkEditor.Editor.Generators.Models;
using SkEditor.Editor.Generators.Services;

namespace SkEditor.Editor.Generators.ViewModels;

public sealed partial class GuiProjectsViewModel : ObservableObject, IDisposable
{
    private DisposableBag _disposables;

    private readonly Func<GuiBlueprint> _blueprintFactory;
    private readonly Subject<GuiBlueprint?> _blueprintLoaded = new();
    private readonly IDialogService _dialogs;
    private readonly IGuiProjectService _projects;
    public Observable<GuiBlueprint?> BlueprintLoaded => _blueprintLoaded;

    public BindableReactiveProperty<string> Filter { get; } = new(string.Empty);
    public NotifyCollectionChangedSynchronizedViewList<GuiProjectItemViewModel> Projects { get; }

    [ObservableProperty]
    public partial GuiProjectItemViewModel? SelectedProject { get; set; }


    public GuiProjectsViewModel(IGuiProjectService projects, IDialogService dialogs,
        Func<GuiBlueprint> blueprintFactory)
    {
        _projects = projects;
        _dialogs = dialogs;
        _blueprintFactory = blueprintFactory;

        ISynchronizedView<GuiProject, GuiProjectItemViewModel> projectsView = projects.Projects
            .CreateView(p =>
                new GuiProjectItemViewModel(p, p.Id == projects.ActiveProject.CurrentValue?.Id))
            .AddTo(ref _disposables);

        Projects = projectsView.ToNotifyCollectionChanged().AddTo(ref _disposables);

        projects.ActiveProject.Subscribe(active =>
        {
            foreach (GuiProjectItemViewModel view in projectsView)
            {
                view.IsActive = view.Model.Id == active?.Id;
            }
        }).AddTo(ref _disposables);

        Filter
            .Subscribe(query =>
            {
                if (string.IsNullOrWhiteSpace(query))
                {
                    projectsView.ResetFilter();
                }
                else
                {
                    projectsView.AttachFilter(item => item.Name.Contains(query, StringComparison.OrdinalIgnoreCase));
                }

                SelectedProject = Projects.FirstOrDefault();
            }).AddTo(ref _disposables);
    }

    public async Task LoadProjects()
    {
        await _projects.LoadProjects();
    }

    [RelayCommand]
    private async Task SaveProject()
    {
        GuiBlueprint blueprint = _blueprintFactory.Invoke();
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

    [RelayCommand]
    private void NewProject()
    {
        _projects.SetActiveProject(null);
        _blueprintLoaded.OnNext(null);
    }

    [RelayCommand]
    private void LoadProject()
    {
        if (SelectedProject is null)
        {
            return;
        }

        _projects.SetActiveProject(SelectedProject.Model);
        _blueprintLoaded.OnNext(SelectedProject.Model.Blueprint);
    }

    [RelayCommand]
    private void DeleteProject()
    {
        if (SelectedProject is null)
        {
            return;
        }

        if (SelectedProject.Model == _projects.ActiveProject.CurrentValue)
        {
            NewProject();
        }

        _projects.DeleteProject(SelectedProject.Model);
    }

    [RelayCommand]
    private async Task RenameProject()
    {
        if (SelectedProject is null)
        {
            return;
        }

        string? promptName = await PromptName("Rename project".ToTranslatable(), "New name".ToTranslatable(),
            SelectedProject.Name);
        string? name = promptName?.Trim();

        if (string.IsNullOrWhiteSpace(name) || name == SelectedProject.Name)
        {
            return;
        }

        await _projects.RenameProject(SelectedProject.Model, name);
    }

    private async Task<string?> PromptName(TranslatableString title, TranslatableString placeholder,
        string? initialValue = null)
    {
        TextBoxDialogElement inputElement = new()
            { Text = initialValue ?? string.Empty, PlaceholderText = placeholder };

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

    public void Dispose()
    {
        _blueprintLoaded.Dispose();
        _disposables.Dispose();
    }
}