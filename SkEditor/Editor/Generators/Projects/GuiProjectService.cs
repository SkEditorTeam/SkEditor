using System;
using System.IO;
using System.IO.Abstractions;
using System.Text.Json;
using System.Threading.Tasks;
using ObservableCollections;
using SkEditor.Core.Common;
using SkEditor.Core.Logging;

namespace SkEditor.Editor.Generators.Projects;

public class GuiProjectService(IFileSystem fileSystem, ILogService logs) : IGuiProjectService
{
    public string FolderPath => fileSystem.Path.Combine(AppPaths.LocalData, "GuiProjects");
    
    public GuiProject? ActiveProject { get; private set; }

    public ObservableList<GuiProject> Projects { get; } = [];
    
    public async Task LoadProjects()
    {
        try
        {
            if (!fileSystem.Directory.Exists(FolderPath))
            {
                return;
            }

            foreach (string file in fileSystem.Directory.EnumerateFiles(FolderPath, "*.json"))
            {
                await using FileSystemStream stream = fileSystem.FileStream.New(file, FileMode.Open);
                GuiProject? project = await JsonSerializer.DeserializeAsync<GuiProject>(stream, GuiProjectContext.Default.GuiProject);
                if (project is null) continue;
                
                Projects.Add(project);
            }
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Saving GUI project failed");
        }
    }

    public async Task SaveProject(GuiProject project)
    {
        try
        {
            fileSystem.Directory.CreateDirectory(FolderPath);
            string path = fileSystem.Path.Combine(FolderPath, $"{project.Id}.json");

            await using FileSystemStream stream = fileSystem.FileStream.New(path, FileMode.Create);
            await JsonSerializer.SerializeAsync(stream, project, GuiProjectContext.Default.GuiProject);
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Saving GUI project failed");
        }
    }

    public void SetActiveProject(GuiProject project)
    {
        ActiveProject = project;
    }
}