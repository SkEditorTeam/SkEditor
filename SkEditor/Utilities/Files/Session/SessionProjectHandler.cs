using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Projects;

namespace SkEditor.Utilities.Files;

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
                JObject projectData = new()
                {
                    ["Version"] = SessionSerializer.Version,
                    ["ProjectPath"] = projectPath,
                    ["Timestamp"] = DateTime.UtcNow.ToString("o")
                };

                string jsonData = projectData.ToString(Formatting.None);
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

            JObject projectData = JObject.Parse(jsonData);
            string? projectPath = projectData["ProjectPath"]?.Value<string>();

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