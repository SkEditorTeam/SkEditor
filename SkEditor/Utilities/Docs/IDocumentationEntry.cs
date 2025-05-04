using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.Utilities.Docs;

/// <summary>
///     Represent a documentation entry. Can be an expression, event, effect...
///     It is mainly used to have a common format for all documentation providers.
/// </summary>
public interface IDocumentationEntry
{
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

    public static List<Type> AllTypes => Enum.GetValues<Type>().ToList();

    #region Events Properties

    public string? EventValues { set; get; }

    #endregion

    #region Visual Utilities

    [JsonIgnore] public string Since => $"Since v{Version}";

    #endregion

    public static IconSource GetTypeIcon(Type type)
    {
        static IBrush GetColor(string key)
        {
            return Application.Current.TryGetResource(key, out object? color) && color is Color parsedColor
                ? new SolidColorBrush(parsedColor)
                : new SolidColorBrush(Colors.Black);
        }

        return type switch
        {
            Type.All => CreateIcon(Symbol.SelectAllOn, "ThemeGreyColor"),
            Type.Event => CreateIcon(Symbol.Call, "ThemeDeepPurpleColor"),
            Type.Expression => CreateIcon(Symbol.DocumentPageNumber, "ThemeMediumSeaGreenColor"),
            Type.Effect => CreateIcon(Symbol.LightbulbFilament, "ThemeLightBlueColorTransparent"),
            Type.Condition => CreateIcon(Symbol.Filter, "ThemeRedColor"),
            Type.Type => CreateIcon(Symbol.Library, "ThemeOrangeColor"),
            Type.Section => CreateIcon(Symbol.NotebookSubsection, "ThemeTealColor"),
            Type.Structure => CreateIcon(Symbol.Code, "ThemeBrownColor"),
            Type.Function => CreateIcon(Symbol.MathFormula, "ThemeBlueGreyColor"),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        static SymbolIconSource CreateIcon(Symbol symbol, string? colorKey = null)
        {
            return new SymbolIconSource
            {
                IconVariant = IconVariant.Filled,
                Symbol = symbol,
                Foreground = colorKey != null ? GetColor(colorKey) : null
            };
        }
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
}