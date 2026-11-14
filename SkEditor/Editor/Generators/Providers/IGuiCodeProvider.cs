using SkEditor.Editor.Generators.Models;

namespace SkEditor.Editor.Generators.Providers;

public interface IGuiCodeProvider
{
    string Name { get; }

    string Generate(GuiBlueprint blueprint);
}