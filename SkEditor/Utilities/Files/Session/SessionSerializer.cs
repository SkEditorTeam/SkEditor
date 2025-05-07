using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;
using SkEditor.API;

namespace SkEditor.Utilities.Files;

public class SessionSerializer
{
    public const string Version = "1.0";

    public static string BuildSavingData(OpenedFile openedFile)
    {
        SkEditorAPI.Logs.Info($"Path: {openedFile.Path}, is saved: {openedFile.IsSaved}");

        JObject obj = new()
        {
            ["Version"] = Version,
            ["Path"] = openedFile.Path,
            ["Content"] = openedFile.Editor?.Text,
            ["HasUnsavedChanges"] = !openedFile.IsSaved,
            ["Timestamp"] = DateTime.UtcNow.ToString("o")
        };

        return obj.ToString(Formatting.None);
    }

    public static SessionFileData BuildOpeningData(string data)
    {
        try
        {
            JObject obj = JObject.Parse(data);

            string? version = obj["Version"]?.Value<string>();
            string? path = obj["Path"]?.Value<string>();
            string? content = obj["Content"]?.Value<string>();
            bool hasUnsavedChanges = obj["HasUnsavedChanges"]?.Value<bool>() ?? false;

            if (version != Version && version != null)
            {
                Log.Warning("Session file version mismatch. Expected {Expected}, got {Actual}",
                    Version, version);
            }

            if (!string.IsNullOrEmpty(content) || string.IsNullOrEmpty(path))
            {
                return new SessionFileData
                {
                    Content = content,
                    Path = path,
                    HasUnsavedChanges = hasUnsavedChanges
                };
            }

            try
            {
                if (File.Exists(path))
                {
                    content = File.ReadAllText(path);
                }
                else
                {
                    Log.Warning("File not found: {Path}", path);
                    content = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to read file: {Path}", path);
                content = string.Empty;
            }

            return new SessionFileData
            {
                Content = content,
                Path = path,
                HasUnsavedChanges = hasUnsavedChanges
            };
        }
        catch (JsonException ex)
        {
            Log.Error(ex, "Error parsing session data");
            return new SessionFileData { Content = string.Empty };
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error building session data");
            return new SessionFileData { Content = string.Empty };
        }
    }

    public class SessionFileData
    {
        public string? Content { get; init; }
        public string? Path { get; set; }
        public bool HasUnsavedChanges { get; init; }
    }
}