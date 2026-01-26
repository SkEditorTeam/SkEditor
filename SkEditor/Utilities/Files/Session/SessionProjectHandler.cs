using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Serilog;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Projects;

namespace SkEditor.Utilities.Files.Session;

public static class SessionProjectHandler
{
    private const string ProjectFileName = "project.json";
    private static readonly string ProjectFilePath = Path.Combine(SessionRestorer.SessionFolder, ProjectFileName);

    public static async Task SaveProjectFolder()
    {
        try
        {
            string? projectPath = ProjectOpener.ProjectRootFolder?.StorageFolderPath;

            if (!string.IsNullOrEmpty(projectPath))
            {
                SessionProjectData sessionProjectData = new()
                {
                    Version = SessionSerializer.Version,
                    ProjectPath = projectPath,
                    Timestamp = DateTime.UtcNow
                };

                string jsonData = JsonSerializer.Serialize(sessionProjectData, SessionJsonContext.Default.SessionProjectData);
                await SessionFileHandler.WriteFile(ProjectFilePath, jsonData);
            }
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to save project folder information");
        }
    }

    public static async Task<bool> RestoreProjectFolder()
    {
        if (!File.Exists(ProjectFilePath))
        {
            return false;
        }

        try
        {
            string jsonData = await SessionFileHandler.ReadFile(ProjectFilePath);
            if (string.IsNullOrEmpty(jsonData))
            {
                return false;
            }

            SessionProjectData? projectData =
                JsonSerializer.Deserialize<SessionProjectData>(jsonData, SessionJsonContext.Default.SessionProjectData);
            string? projectPath = projectData?.ProjectPath;

            projectPath = projectPath?.NormalizePathSeparators();

            if (string.IsNullOrEmpty(projectPath))
            {
                Log.Warning("The project path is empty!");
                return false;
            }

            await ProjectOpener.OpenProject(projectPath);
            return true;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to restore project folder");
            return false;
        }
    }
}

public class SessionProjectData
{
    public string? Version { get; set; }
    public string? ProjectPath { get; set; }
    public DateTime? Timestamp { get; set; }
}