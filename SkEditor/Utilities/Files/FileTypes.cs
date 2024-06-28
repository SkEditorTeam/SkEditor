using Avalonia.Media.Imaging;
using AvaloniaEdit;
using SkEditor.API;
using SkEditor.Views.FileTypes;
using System;
using System.Collections.Generic;
using System.IO;

namespace SkEditor.Utilities.Files;

/// <summary>
/// Class handling opening of non-editor files (images, audio, etc.)
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
            SkEditorAPI.Windows.ShowError($"Unable to register file association for {association.GetType().Name}:\n\nAddon is null");
            return;
        }

        RegisterAssociation(association);
    }

    private static void RegisterAssociation(FileAssociation association)
    {
        foreach (var extension in association.SupportedExtensions)
        {
            if (!RegisteredFileTypes.ContainsKey(extension))
                RegisteredFileTypes.Add(extension, new List<FileAssociation>());

            RegisteredFileTypes[extension].Add(association);
        }
    }

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

        public List<string> SupportedExtensions { get; set; }
        public bool IsFromAddon { get; set; } = false;

        public IAddon? Addon { get; set; } = null;

        public abstract FileType? Handle(string path);
    }

    #endregion

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
                var fileStream = File.OpenRead(Uri.UnescapeDataString(path));
                var bitmap = new Bitmap(fileStream);
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
}