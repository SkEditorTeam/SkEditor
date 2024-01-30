using System.Collections.Generic;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities.Files;

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