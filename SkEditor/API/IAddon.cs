using System.Collections.Generic;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Symbol = FluentIcons.Common.Symbol;
using SymbolIconSource = FluentIcons.Avalonia.Fluent.SymbolIconSource;

namespace SkEditor.API;

/// <summary>
/// Base class for an SkEditor addon.
/// </summary>
public interface IAddon
{
    /// <summary>
    /// The name of the addon.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The version of the addon. Should follow <see href="https://semver.org/">Semantic Versioning,</see>
    /// so it can be parsed into a <see cref="System.Version"/>.
    /// </summary>
    public string Version { get; }
    
    /// <summary>
    /// A short description of the addon.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// The icon of the addon.
    /// </summary>
    /// <returns>The icon of the addon.</returns>
    public virtual IconSource GetAddonIcon()
    {
        return new SymbolIconSource() { Symbol = Symbol.AppsAddIn };
    }

    /// <summary>
    /// The menu items of the addon. This will automatically be added to the 'Addons'
    /// menu by SkEditor, so you don't have to worry about conflicts.
    /// </summary>
    /// <returns>The menu items of the addon.</returns>
    public virtual List<MenuItem> GetMenuItems()
    {
        return [];
    }
    
    // ------------------- Events

    /// <summary>
    /// Called when the addon is enabled. Some stuff might not be available
    /// yet, but mainly <see cref="Registry.Registries"/> will be available,
    /// so you can register your own stuff there.
    /// </summary>
    public void OnEnable();
    
    /// <summary>
    /// Called when every addons has been enabled, and the
    /// first lifecycle event has been called. This is when you
    /// can do UI-related things for instance.
    /// </summary>
    public virtual void OnPostEnable() { }
    
    /// <summary>
    /// Called when the addon is disabled. You <b>do not need</b> to
    /// unregister your stuff from <see cref="Registry.Registries"/>, as
    /// SkEditor will do that for you!
    /// </summary>
    public virtual void OnDisable() { }
}