using System;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;

namespace SkEditor.API;

/// <summary>
/// Represent a file type, that is a collection of supported extensions and a file opener.
/// One file opener can usually open multiple file types, for instance, the image viewer
/// that can open .png, .jpg, .jpeg, .bmp, etc.
/// </summary>
/// <param name="SupportedExtensions">List of supported extensions, <strong>without</strong> the dot.</param>
/// <param name="FileOpener">The function that opens the file. The function should return a Control, or a null value if the file could not be opened.</param>
/// <param name="Description">The description of the file type, can be null although it is recommended to provide one.</param>
public record FileTypeData(
    string[] SupportedExtensions,
    Func<string, FileTypeResult?> FileOpener,
    string? Description = null)
{

    #region UI Usages

    public IconSource Icon => Registries.FileTypes.GetValueKey(this).Addon.GetAddonIcon();
    public string AddonName => Registries.FileTypes.GetValueKey(this).Addon.Name;
    public string DisplayedDescription => this.Description ?? "No description provided.";

    #endregion

};
    
public record FileTypeResult(Control? Control, string? Header = null);