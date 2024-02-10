using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using System.Collections.Generic;

namespace SkEditor.API;
public interface IAddon
{
    public string Name { get; }
    public string Version { get; }

    public virtual List<MenuItem> GetMenuItems()
    {
        return [];
    }

    public virtual Symbol GetMenuIcon()
    {
        return Symbol.Document;
    }

    public void OnEnable();
}