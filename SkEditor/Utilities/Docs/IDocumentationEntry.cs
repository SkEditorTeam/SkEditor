using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Utilities.Docs;

/// <summary>
/// Represent a documentation entry. Can be an expression, event, effect...
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

    public static IconSource GetTypeIcon(Type type)
    {
        IBrush GetColor(string key)
        {
            Application.Current.TryGetResource(key, out var color);
            return new SolidColorBrush(color is Color parsedColor ? parsedColor : Colors.Black);
        }

        return type switch
        {
            Type.All => new SymbolIconSource() { IsFilled = true, Symbol = Symbol.BorderAll },
            Type.Event => new SymbolIconSource() { IsFilled = true, Symbol = Symbol.Call, Foreground = GetColor("ThemeDeepPurpleColor") },
            Type.Expression => new SymbolIconSource() { IsFilled = true, Symbol = Symbol.DocumentPageNumber, Foreground = GetColor("ThemeMediumSeaGreenColor") },
            Type.Effect => new SymbolIconSource() { IsFilled = true, Symbol = Symbol.LightbulbFilament, Foreground = GetColor("ThemeLightBlueColorTransparent") },
            Type.Condition => new SymbolIconSource() { IsFilled = true, Symbol = Symbol.Filter, Foreground = GetColor("ThemeRedColor") },
            Type.Type => new SymbolIconSource() { IsFilled = true, Symbol = Symbol.Library, Foreground = GetColor("ThemeOrangeColor") },
            Type.Section => new SymbolIconSource() { IsFilled = true, Symbol = Symbol.NotebookSubsection, Foreground = GetColor("ThemeTealColor") },
            Type.Structure => new SymbolIconSource() { IsFilled = true, Symbol = Symbol.Code, Foreground = GetColor("ThemeBrownColor") },
            Type.Function => new SymbolIconSource() { IsFilled = true, Symbol = Symbol.MathFormula, Foreground = GetColor("ThemeBlueGreyColor") },
            _ => throw new ArgumentOutOfRangeException()
        };
    }

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