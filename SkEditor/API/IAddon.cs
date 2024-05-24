using System.Collections.Generic;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.API.Registry;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIcon = FluentIcons.Avalonia.Fluent.SymbolIcon;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.API;
public interface IAddon
{
    public string Name { get; }
    public string Version { get; }
    public string? Description { get; }

    public virtual List<MenuItem> GetMenuItems()
    {
        return [];
    }

    public virtual IconSource GetAddonIcon()
    {
        return new SymbolIconSource() { Symbol = Symbol.AppsAddIn };
    }

    public void OnEnable();
}