using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Serilog;
using SkEditor.API;

namespace SkEditor.Utilities.Files;

public static class SessionRestorer
{
    private static readonly string SessionFolder = Path.Combine(Path.GetTempPath(), "SkEditor", "Session");

    public static async Task SaveSession()
    {
        if (Directory.Exists(SessionFolder))
        {
            Directory.Delete(SessionFolder, true);
        }

        Directory.CreateDirectory(SessionFolder);

        List<OpenedFile> openedFiles = SkEditorAPI.Files.GetOpenedFiles();
        int index = 0;
        foreach (OpenedFile openedFile in openedFiles)
        {
            if (!openedFile.IsEditor || string.IsNullOrEmpty(openedFile.Editor.Text))
            {
                continue;
            }

            string jsonData = BuildSavingData(openedFile);
            string compressed = await Compress(jsonData);
            string path = Path.Combine(SessionFolder, $"file_{index}.skeditor");
            await File.WriteAllTextAsync(path, compressed);
            index++;
        }
    }

    public static async Task<bool> RestoreSession()
    {
        if (!Directory.Exists(SessionFolder))
        {
            return false;
        }

        string[] files = Directory.GetFiles(SessionFolder);
        if (files.Length == 0)
        {
            return false;
        }

        foreach (string file in files)
        {
            string compressed = await File.ReadAllTextAsync(file);
            string jsonData = await Decompress(compressed);
            if (string.IsNullOrEmpty(jsonData))
            {
                continue;
            }

            (string Content, string? Path, bool HasUnsavedChanges) data = BuildOpeningData(jsonData);
            await SkEditorAPI.Files.AddEditorTab(data.Content, data.Path);

            if (data.HasUnsavedChanges)
            {
                OpenedFile? openedFile = SkEditorAPI.Files.GetOpenedFileByPath(data.Path);
                if (openedFile != null)
                {
                    openedFile.IsSaved = false;
                }
            }
        }

        return true;
    }

    #region Compressing/Decompressing

    private static async Task<string> Compress(string data)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(data);
        using MemoryStream ms = new();
        await using (GZipStream sw = new(ms, CompressionMode.Compress))
        {
            sw.Write(byteArray, 0, byteArray.Length);
        }

        return Convert.ToBase64String(ms.ToArray());
    }

    private static async Task<string> Decompress(string data)
    {
        string result = string.Empty;
        try
        {
            byte[] byteArray = Convert.FromBase64String(data);
            using MemoryStream ms = new(byteArray);
            await using GZipStream sr = new(ms, CompressionMode.Decompress);
            using StreamReader reader = new(sr);
            result = await reader.ReadToEndAsync();
        }
        catch (FormatException e)
        {
            Log.Warning(e, "Error while decompressing data");
        }

        return result;
    }

    #endregion

    #region Serialization/Deserialization

    private static string BuildSavingData(OpenedFile openedFile)
    {
        JObject obj = new()
        {
            ["Path"] = openedFile.Path,
            ["Content"] = openedFile.Editor.Text,
            ["HasUnsavedChanges"] = !openedFile.IsSaved
        };

        return obj.ToString();
    }

    private static (string Content, string? Path, bool HasUnsavedChanges) BuildOpeningData(string data)
    {
        JObject obj = JObject.Parse(data);

        string? path = obj["Path"]?.Value<string>();
        string? content = obj["Content"]?.Value<string>();
        bool hasUnsavedChanges = obj["HasUnsavedChanges"]?.Value<bool>() ?? false;

        if (string.IsNullOrEmpty(content) && !string.IsNullOrEmpty(path))
        {
            content = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        }

        return (content, path, hasUnsavedChanges);
    }

    #endregion
}