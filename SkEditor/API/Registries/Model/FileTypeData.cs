using System;
using Avalonia.Controls;

namespace SkEditor.API;

/// <summary>
/// Represent a file type, that is a collection of supported extensions and a file opener.
/// One file opener can usually open multiple file types, for instance, the image viewer
/// that can open .png, .jpg, .jpeg, .bmp, etc.
/// </summary>
/// <param name="SupportedExtensions">List of supported extensions, <strong>without</strong> the dot.</param>
/// <param name="FileOpener">The function that opens the file. The function should return a Control, or a null value if the file could not be opened.</param>
public record FileTypeData(string[] SupportedExtensions,
    Func<string, FileTypeResult?> FileOpener);
    
public record FileTypeResult(Control? Control, string? Header = null);