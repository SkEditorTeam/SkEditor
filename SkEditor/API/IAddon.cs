using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using SkEditor.API.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
    /// Get the unique identifier of the addon. Must be alphanumeric!
    /// </summary>
    public string Identifier { get; }

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

    /// <summary>
    /// Called when the addon is enabled. Some stuff might not be available
    /// yet, but mainly <see cref="Registries"/> will be available,
    /// so you can register your own stuff there.
    /// </summary>
    public void OnEnable();

    /// <summary>
    /// Called when the addon is enabled. Some stuff might not be available
    /// yet, but mainly <see cref="Registries"/> will be available,
    /// so you can register your own stuff there.
    ///
    /// Use this method if you need to do async stuff when enabling the addon.
    /// </summary>
    public virtual Task OnEnableAsync()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called when the addon is disabled. You <b>do not need</b> to
    /// unregister your stuff from <see cref="Registries"/>, as
    /// SkEditor will do that for you!
    /// </summary>
    public virtual void OnDisable() { }

    /// <summary>
    /// Get the minimal version of SkEditor that is required for this addon.
    /// </summary>
    /// <returns>The minimal version of SkEditor that is required for this addon.</returns>
    public Version GetMinimalSkEditorVersion();

    /// <summary>
    /// Get the maximal version of SkEditor that is required for this addon.
    /// If null, there is no maximal version.
    /// </summary>
    /// <returns>The maximal version of SkEditor that is required for this addon, or null if there is no maximal version.</returns>
    public virtual Version? GetMaximalSkEditorVersion() => null;

    /// <summary>
    /// Get all the dependencies of this addon. Those can either be
    /// <see cref="AddonDependency"/>s or <see cref="NuGetDependency"/>s.
    /// </summary>
    /// <returns>All the dependencies of this addon.</returns>
    public virtual List<IDependency> GetDependencies() => [];

    /// <summary>
    /// Get all the settings of this addon.
    /// </summary>
    /// <typeparam name="T">The type of the setting.</typeparam>
    /// <returns>All the settings of this addon.</returns>
    public virtual List<Setting> GetSettings() => [];

}