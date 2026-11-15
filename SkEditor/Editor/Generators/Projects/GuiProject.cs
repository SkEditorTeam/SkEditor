using System;
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
public partial class GuiProjectContext : JsonSerializerContext;