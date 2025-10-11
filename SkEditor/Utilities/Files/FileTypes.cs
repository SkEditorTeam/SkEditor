using System;
using System.Collections.Generic;
using System.IO;
using Avalonia.Media.Imaging;
using AvaloniaEdit;
using SkEditor.API;
using ImageViewer = SkEditor.Views.Windows.FileTypes.Images.ImageViewer;

namespace SkEditor.Utilities.Files;

/// <summary>
///     Class handling opening of non-editor files (images, audio, etc.)
/// </summary>
public static class FileTypes
{
    public static readonly Dictionary<string, List<FileAssociation>> RegisteredFileTypes = new();

    public static void RegisterDefaultAssociations()
    {
        RegisterAssociation(new ImageAssociation());
    }

    public static void RegisterExternalAssociation(FileAssociation association)
    {
        association.IsFromAddon = true;
        if (association.Addon == null)
        {
            SkEditorAPI.Windows.ShowError(
                $"Unable to register file association for {association.GetType().Name}:\n\nAddon is null");
            return;
        }

        RegisterAssociation(association);
    }

    private static void RegisterAssociation(FileAssociation association)
    {
        if (association.SupportedExtensions == null)
        {
            return;
        }

        foreach (string extension in association.SupportedExtensions)
        {
            if (!RegisteredFileTypes.TryGetValue(extension, out List<FileAssociation>? value))
            {
                value = [];
                RegisteredFileTypes.Add(extension, value);
            }

            value.Add(association);
        }
    }

    #region Default File Associations

    public class ImageAssociation : FileAssociation
    {
        public ImageAssociation()
        {
            SupportedExtensions = [".png", ".jpg", ".ico", ".jpeg", ".gif", ".bmp", ".tiff", ".webp"];
        }

        public override FileType? Handle(string path)
        {
            try
            {
                FileStream fileStream = File.OpenRead(Uri.UnescapeDataString(path));
                Bitmap bitmap = new(fileStream);
                fileStream.Close();

                return new FileType(new ImageViewer(bitmap, path), path);
            }
            catch (Exception e)
            {
                SkEditorAPI.Windows.ShowError($"Unable to load the specified image:\n\n{e.Message}");
                return null;
            }
        }
    }

    #endregion

    #region Classes

    public class FileType(object display, string path, bool needsBottomBar = false)
    {
        public object Display { get; set; } = display;
        public string Path { get; set; } = path;
        public bool NeedsBottomBar { get; set; } = needsBottomBar;
        public bool IsEditor => Display is TextEditor;
    }

    public abstract class FileAssociation
    {
        public List<string>? SupportedExtensions { get; protected set; }
        public bool IsFromAddon { get; set; }

        public IAddon? Addon { get; set; } = null;

        public abstract FileType? Handle(string path);
    }

    #endregion
}