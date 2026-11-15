using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators.Projects;

public class GuiProject
{
    public int Version { get; init; } = 1;
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset ModifyDate { get; set; }
    public required string Name { get; set; }
    public required GuiBlueprint Blueprint { get; set; }
}

[JsonSerializable(typeof(GuiProject))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class GuiProjectContext : JsonSerializerContext
{
    public static GuiProjectContext Relaxed { get; } = new(new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}