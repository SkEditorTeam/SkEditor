using System;

namespace SkEditor.Utilities.Docs;

/// <summary>
/// Represent a documentation entry. Can be an expression, event, effect ...
/// It is mainly used to have a common format for all documentation providers.
/// </summary>
public interface IDocumentationEntry
{
    
    public enum Type
    {
        Event,
        Expression,
        Effect,
        Condition,
        Type,
        Section,
        Structure,
        Function
    }

    #region Common Properties

    public string Name { get; }
    public string Description { get; }
    public string Patterns { get; }
    public string Id { get; }
    public string Addon { get; }
    public string Version { get; }
    public Type DocType { get; }
    public DocProvider Provider { get; }

    #endregion

    #region Expressions Properties

    public string? ReturnType { get; }
    public string? Changers { get; }

    #endregion

    #region Events Properties

    public string? EventValues { get; }

    #endregion
    
}