using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using FluentAvalonia.UI.Controls;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Utilities.Docs;

/// <summary>
/// Represent a documentation entry. Can be an expression, event, effect ...
/// It is mainly used to have a common format for all documentation providers.
/// </summary>
public interface IDocumentationEntry
{

    public static List<Type> AllTypes => Enum.GetValues<Type>().ToList();

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
    
    public static IconSource GetTypeIcon(Type type) => type switch
    {
        Type.All => new SymbolIconSource() { Symbol = Symbol.BorderAll },
        Type.Event => new SymbolIconSource() { Symbol = Symbol.Call },
        Type.Expression => new SymbolIconSource() { Symbol = Symbol.DocumentPageNumber },
        Type.Effect => new SymbolIconSource() { Symbol = Symbol.LightbulbFilament },
        Type.Condition => new SymbolIconSource() { Symbol = Symbol.Filter },
        Type.Type => new SymbolIconSource() { Symbol = Symbol.Library },
        Type.Section => new SymbolIconSource() { Symbol = Symbol.NotebookSubsection },
        Type.Structure => new SymbolIconSource() { Symbol = Symbol.Code },
        Type.Function => new SymbolIconSource() { Symbol = Symbol.MathFormula },
        _ => throw new ArgumentOutOfRangeException()
    };

    public enum Changer
    {
        Set,
        Add,
        Remove,
        RemoveAll,
        Reset,
        Clear,
        Delete
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