namespace SkEditor.Utilities.Parser;

/// <summary>
///     Represent a parsed code element that is named (variables, functions, etc) and thus can be renamed.
/// </summary>
public interface INameableCodeElement
{
    string Name { get; }

    void Rename(string newName);

    string GetNameDisplay()
    {
        return Name;
    }
}