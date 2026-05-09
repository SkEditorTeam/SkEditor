using FluentIcons.Common;

namespace SkEditor.Models.Projects;

public static class ProjectMenuItems
{
    public static ContextMenuItem NewFile => ContextMenuItem.Item("New File").WithIcon(Symbol.DocumentAdd).AllowOneItemOnly();
    public static ContextMenuItem NewFolder => ContextMenuItem.Item("New Folder").WithIcon(Symbol.FolderAdd).AllowOneItemOnly();
    
    public static ContextMenuItem CopyPath => ContextMenuItem.Item("Copy Path").WithIcon(Symbol.ClipboardLink);
    public static ContextMenuItem CopyRelativePath => ContextMenuItem.Item("Copy Relative Path").WithIcon(Symbol.ClipboardArrowRight);
    
    public static ContextMenuItem Duplicate => ContextMenuItem.Item("Duplicate").WithIcon(Symbol.DocumentCopy);
    public static ContextMenuItem Rename => ContextMenuItem.Item("Rename").WithIcon(Symbol.Rename).AllowOneItemOnly();
    public static ContextMenuItem Delete => ContextMenuItem.Item("Delete").WithIcon(Symbol.Delete);
    
    public static ContextMenuItem ShowInExplorer => ContextMenuItem.Item("Show in Explorer").WithIcon(Symbol.FolderOpen);
    public static ContextMenuItem OpenInTerminal => ContextMenuItem.Item("Open in Terminal").WithIcon(Symbol.WindowConsole).AllowOneItemOnly();
    
    public static ContextMenuItem Upload => ContextMenuItem.Item("Upload").WithIcon(Symbol.ArrowUpload).AllowOneItemOnly();
    public static ContextMenuItem Download => ContextMenuItem.Item("Download").WithIcon(Symbol.ArrowDownload);
    
    public static ContextMenuItem Refresh => ContextMenuItem.Item("Refresh").WithIcon(Symbol.ArrowClockwise);
    public static ContextMenuItem Disconnect => ContextMenuItem.Item("Disconnect").WithIcon(Symbol.PlugDisconnected);
}