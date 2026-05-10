using FluentIcons.Common;
using System.Collections.Generic;

namespace SkEditor.Models.Projects;

public record MenuTemplate(string Header, Symbol Icon, string? InputGestureText = null, bool AllowOneItemOnly = false);

public static class ProjectMenuRegistry
{
    public static readonly Dictionary<ProjectAction, MenuTemplate> Templates = new()
    {
        { ProjectAction.Open, new MenuTemplate("Open", Symbol.Open) },
        { ProjectAction.NewFile, new MenuTemplate("New File", Symbol.DocumentAdd, AllowOneItemOnly: true) },
        { ProjectAction.NewFolder, new MenuTemplate("New Folder", Symbol.FolderAdd, AllowOneItemOnly: true) },
        
        { ProjectAction.CopyPath, new MenuTemplate("Copy Path", Symbol.ClipboardLink) },
        { ProjectAction.CopyRelativePath, new MenuTemplate("Copy Relative Path", Symbol.ClipboardArrowRight) },
        
        { ProjectAction.Duplicate, new MenuTemplate("Duplicate", Symbol.DocumentCopy) },
        { ProjectAction.Rename, new MenuTemplate("Rename", Symbol.Rename, "F2", AllowOneItemOnly: true) },
        { ProjectAction.Delete, new MenuTemplate("Delete", Symbol.Delete) },
        
        { ProjectAction.ShowInExplorer, new MenuTemplate("Show in Explorer", Symbol.FolderOpen) },
        
        { ProjectAction.Upload, new MenuTemplate("Upload", Symbol.ArrowUpload, AllowOneItemOnly: true) },
        { ProjectAction.Download, new MenuTemplate("Download", Symbol.ArrowDownload) },
        
        { ProjectAction.Refresh, new MenuTemplate("Refresh", Symbol.ArrowClockwise) },
        { ProjectAction.Disconnect, new MenuTemplate("Disconnect", Symbol.PlugDisconnected) },
    };
}