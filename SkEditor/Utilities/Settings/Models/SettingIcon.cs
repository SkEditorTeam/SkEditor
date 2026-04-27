using FluentIcons.Common;

namespace SkEditor.Utilities.Settings.Models;

public readonly record struct SettingIcon
{
    private SettingIcon(object value) => Value = value;
    public object Value { get; init; }

    public bool IsSymbol => Value is Symbol;
    public bool IsPath => Value is string;

    public static implicit operator SettingIcon(Symbol symbol)
    {
        return new SettingIcon(symbol);
    }

    public static implicit operator SettingIcon(string path)
    {
        return new SettingIcon(path);
    }
}