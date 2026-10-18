using System.Collections.Generic;
using FluentIcons.Common;
using SkEditor.Core.Localization;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Workspace.Projects.Models;

public record MenuTemplate(
    TranslatableString Header,
    Symbol Icon,
    string? InputGestureText = null,
    bool AllowOneItemOnly = false);

// todo: complete the commented actions
public static class ProjectMenuRegistry
{
    public static readonly Dictionary<ProjectAction, MenuTemplate> Templates = new()
    {
        { ProjectAction.Open, new MenuTemplate(TranslationKey.MenuHeaderOpen, Symbol.Open) },
        {
            ProjectAction.NewFile,
            new MenuTemplate(TranslationKey.MenuHeaderNewFile, Symbol.DocumentAdd, "Ctrl+N", true)
        },
        {
            ProjectAction.NewFolder,
            new MenuTemplate(TranslationKey.MenuHeaderNewFolder, Symbol.FolderAdd, "Ctrl+Shift+N", true)
        },

        { ProjectAction.CopyPath, new MenuTemplate(TranslationKey.MenuHeaderCopyAbsolutePath, Symbol.ClipboardLink) },
        {
            ProjectAction.CopyRelativePath,
            new MenuTemplate(TranslationKey.MenuHeaderCopyPath, Symbol.ClipboardArrowRight)
        },

        // { ProjectAction.Duplicate, new MenuTemplate("Duplicate", Symbol.DocumentCopy) },
        { ProjectAction.Rename, new MenuTemplate(TranslationKey.MenuHeaderRename, Symbol.Rename, "F2", true) },
        { ProjectAction.Delete, new MenuTemplate(TranslationKey.MenuHeaderDelete, Symbol.Delete, "Delete") },

        { ProjectAction.ShowInExplorer, new MenuTemplate(TranslationKey.MenuHeaderOpenInExplorer, Symbol.FolderOpen) },

        //{ ProjectAction.Upload, new MenuTemplate("Upload", Symbol.ArrowUpload, AllowOneItemOnly: true) },
        //{ ProjectAction.Download, new MenuTemplate("Download", Symbol.ArrowDownload) },

        {
            ProjectAction.Refresh,
            new MenuTemplate(TranslationKey.MenuHeaderRefresh, Symbol.ArrowClockwise, AllowOneItemOnly: true)
        },
        { ProjectAction.Disconnect, new MenuTemplate(TranslationKey.MenuHeaderDisconnect, Symbol.PlugDisconnected) },

        { ProjectAction.Close, new MenuTemplate(TranslationKey.MenuHeaderClose, Symbol.Dismiss) }
    };
}