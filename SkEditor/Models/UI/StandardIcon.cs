using FluentIcons.Common;

namespace SkEditor.Models.UI;

/// <summary>
///     Represents an icon that can be either a symbol or a path icon (represented by a string resource name)
/// </summary>
public readonly record struct StandardIcon
{
    public bool IsPath => Value is string;

    public bool IsSymbol => Value is Symbol;
    public object Value { get; init; }
    private StandardIcon(object value) => Value = value;

    public static implicit operator StandardIcon(Symbol symbol)
    {
        return new StandardIcon(symbol);
    }

    public static implicit operator StandardIcon(string path)
    {
        return new StandardIcon(path);
    }
}