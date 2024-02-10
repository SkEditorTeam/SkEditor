namespace SkEditor.Utilities.Parser;

/// <summary>
/// Represent a parsed code element that is named (variables, functions, etc) and thus can be renamed.
/// </summary>
public interface INameableCodeElement
{

    public string Name { get; }

    public void Rename(string newName);

    public virtual string GetNameDisplay()
    {
        return Name;
    }

}