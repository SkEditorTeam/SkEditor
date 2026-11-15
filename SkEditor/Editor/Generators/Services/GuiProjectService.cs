using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using ObservableCollections;
using R3;
using SkEditor.Core.Common;
using SkEditor.Core.Logging;
using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators.Services;

public class GuiProjectService(IFileSystem fileSystem, TimeProvider timeProvider, ILogService logs) : IGuiProjectService
{
    public string FolderPath => fileSystem.Path.Combine(AppPaths.LocalData, "GuiProjects");

    private readonly ReactiveProperty<GuiProject?> _activeProject = new(null);
    public ReadOnlyReactiveProperty<GuiProject?> ActiveProject => _activeProject;

    public ObservableList<GuiProject> Projects { get; } = [];

    public async Task LoadProjects()
    {
        try
        {
            if (!fileSystem.Directory.Exists(FolderPath))
            {
                return;
            }

            Projects.Clear();
            List<GuiProject> loadedProjects = [];

            FileStreamOptions options = new()
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.ReadWrite | FileShare.Delete,
                Options = FileOptions.Asynchronous | FileOptions.SequentialScan
            };

            foreach (string file in fileSystem.Directory.EnumerateFiles(FolderPath, "*.json"))
            {
                try
                {
                    await using FileSystemStream stream = fileSystem.FileStream.New(file, options);
                    GuiProject? project =
                        await JsonSerializer.DeserializeAsync<GuiProject>(stream, GuiProjectContext.Default.GuiProject);
                    if (project is null) continue;

                    loadedProjects.Add(project);
                }
                catch (Exception ex)
                {
                    logs.Error(ex, $"Loading project {file} failed");
                }
            }

            Projects.AddRange(loadedProjects.OrderByDescending(p => p.ModifyDate));
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Loading GUI project failed");
        }
    }

    public async Task SaveProject(GuiProject project)
    {
        try
        {
            project.ModifyDate = timeProvider.GetUtcNow();

            await SaveFile(project);

            GuiProject? existing = Projects.FirstOrDefault(p => p.Id == project.Id);
            if (existing is not null)
            {
                Projects.Remove(existing);
            }
            
            Projects.Insert(0, project);
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Saving GUI project failed");
        }
    }

    public void SetActiveProject(GuiProject? project)
    {
        _activeProject.Value = project;
    }

    public void DeleteProject(GuiProject project)
    {
        try
        {
            string path = fileSystem.Path.Combine(FolderPath, $"{project.Id}.json");

            if (fileSystem.File.Exists(path))
            {
                fileSystem.File.Delete(path);
            }

            Projects.Remove(project);

            if (_activeProject.Value?.Id == project.Id)
            {
                SetActiveProject(null);
            }
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Deleting GUI project failed");
        }
    }

    public async Task RenameProject(GuiProject project, string name)
    {
        project.Name = name;
        await SaveFile(project);
    }
    
    private async Task SaveFile(GuiProject project)
    {
        fileSystem.Directory.CreateDirectory(FolderPath);
        string path = fileSystem.Path.Combine(FolderPath, $"{project.Id}.json");
        string tmpPath = path + ".tmp";

        await using (FileSystemStream
                     stream = fileSystem.FileStream.New(tmpPath, FileMode.Create, FileAccess.Write))
        {
            await JsonSerializer.SerializeAsync(stream, project, Models.GuiProjectContext.Relaxed.GuiProject);
        }

        fileSystem.File.Move(tmpPath, path, overwrite: true);
    }
}