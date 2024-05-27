namespace SkEditor.Parser.Elements;

/// <summary>
/// Represent an element that can be parsed
/// from a <see cref="Node"/> to a <see cref="Element"/>.
///
/// It must have a <b>static Parse</b> method that takes a <see cref="Node"/>
/// and returns a boolean indicating if the parsing was successful/is possible.
///
/// For any other "loading" logic, it should be done in the Load method.
/// </summary>
public abstract class Element
{
    
    /// <summary>
    /// Load the element's data from a <see cref="Node"/>.
    /// Type of node should be checked in the Parse method instead.
    /// </summary>
    /// <param name="node">The node to load the data from.</param>
    public abstract void Load(Node node);
    
}