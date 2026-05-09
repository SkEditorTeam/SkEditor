using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using AvaloniaEdit.Utils;
using SkEditor.Models.Projects;
using SkEditor.Utilities;

namespace SkEditor.ViewModels.Controls;

public partial class FileTreeViewModel
{
    private Dictionary<ProjectAction, Action<List<FileSystemNode>>> _commandMap;
    
    [MemberNotNull(nameof(_commandMap))]
    private void InitializeCommandMap()
    {
        _commandMap = new Dictionary<ProjectAction, Action<List<FileSystemNode>>>
        {
            { ProjectAction.ShowInExplorer, OpenInExplorer },
        };
    }
    
    private static void OpenInExplorer(List<FileSystemNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node.IsDirectory)
                LinkOpener.OpenFolder(node.Path);
            else
                LinkOpener.OpenFolderWithFileSelection(node.Path);
        }
    }
    
    private void UpdateContextMenuItems()
    {
        ContextMenuItems.Clear();
        if (SelectedNodes.Count == 0) return;
        
        var selectedNodes = SelectedNodes.ToList();

        var actionsTemplate = SelectedNodes.First().GetSupportedActions();
        var commonActions = SelectedNodes
            .Select(n => n.GetSupportedActions().Where(a => a != ProjectAction.Separator))
            .Aggregate((prev, next) => prev.Intersect(next))
            .ToHashSet();

        foreach (var action in actionsTemplate)
        {
            if (action == ProjectAction.Separator)
            {
                ContextMenuItems.Add(ContextMenuItem.Separator);
                continue;
            }

            if (!commonActions.Contains(action)) continue;

            var template = ProjectMenuRegistry.Templates[action];
        
            if (template.AllowOneItemOnly && SelectedNodes.Count > 1) continue;

            _commandMap.TryGetValue(action, out Action<List<FileSystemNode>>? handler);
            handler ??= _ => { };
            var item = ContextMenuItem.Item(template.Header)
                .WithIcon(template.Icon)
                .WithCommand(() => handler(selectedNodes));

            ContextMenuItems.Add(item);
        }

        var cleaned = CleanUpSeparators(ContextMenuItems.ToList());
        ContextMenuItems.Clear();
        ContextMenuItems.AddRange(cleaned);
    }

    private static List<ContextMenuItem> CleanUpSeparators(IEnumerable<ContextMenuItem> menuItems)
    {
        List<ContextMenuItem> cleaned = [];
        bool lastWasSeparator = true;

        foreach (ContextMenuItem item in menuItems)
        {
            if (item.IsSeparator)
            {
                if (lastWasSeparator)
                {
                    continue;
                }

                cleaned.Add(item);
                lastWasSeparator = true;
            }
            else
            {
                cleaned.Add(item);
                lastWasSeparator = false;
            }
        }

        if (cleaned.Count > 0 && cleaned[^1].IsSeparator)
        {
            cleaned.RemoveAt(cleaned.Count - 1);
        }

        return cleaned;
    }
}