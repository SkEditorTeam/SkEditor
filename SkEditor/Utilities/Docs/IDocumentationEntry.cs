using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Avalonia.Controls;

namespace SkEditor.Utilities.Docs;

/// <summary>
/// Represent a documentation entry. Can be an expression, event, effect ...
/// It is mainly used to have a common format for all documentation providers.
/// </summary>
public interface IDocumentationEntry
{

    public static ItemsSourceView<Type> AllTypes => ItemsSourceView.GetOrCreate(
        Enum.GetValues<Type>().ToList());
    
    public enum Type
    {
        All,
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

    public string Name { set; get; }
    public string Description { set; get; }
    public string Patterns { set; get; }
    public string Id { set; get; }
    public string Addon { set; get; }
    public string Version { set; get; }
    public Type DocType { set; get; }
    public DocProvider Provider { get; }

    #endregion

    #region Expressions Properties

    public string? ReturnType { set; get; }
    public string? Changers { set; get; }

    #endregion

    #region Events Properties

    public string? EventValues { set; get; }

    #endregion

    #region Visual Utilities

    [JsonIgnore] public string Since => $"Since v{Version}";

    #endregion
    
}