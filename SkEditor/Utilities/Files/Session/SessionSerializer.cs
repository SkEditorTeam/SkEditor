using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

namespace SkEditor.Utilities.Files.Session;

public class SessionSerializer
{
    public const string Version = "1.0";

    public static string BuildSavingData(OpenedFile openedFile)
    {
        SessionData data = new()
        {
            Version = Version,
            Path = openedFile.Path,
            Content = openedFile.Editor?.Text,
            HasUnsavedChanges = !openedFile.IsSaved,
            Timestamp = DateTime.UtcNow
        };

        return JsonSerializer.Serialize(data, SessionJsonContext.Default.SessionData);
    }

    public static SessionFileData BuildOpeningData(string data)
    {
        try
        {
            SessionData? sessionData = JsonSerializer.Deserialize(data, SessionJsonContext.Default.SessionData);
            if (sessionData == null)
            {
                Log.Error("Deserialized session data is null");
                return new SessionFileData { Content = string.Empty };
            }

            string? version = sessionData.Version;
            string? path = sessionData.Path;
            string? content = sessionData.Content;
            bool hasUnsavedChanges = sessionData.HasUnsavedChanges;

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

public class SessionData
{
    public string? Version { get; set; }
    public string? Path { get; set; }
    public string? Content { get; set; }
    public bool HasUnsavedChanges { get; set; }
    public DateTime? Timestamp { get; set; }
}

[JsonSerializable(typeof(SessionData))]
[JsonSerializable(typeof(SessionProjectData))]
public partial class SessionJsonContext : JsonSerializerContext;