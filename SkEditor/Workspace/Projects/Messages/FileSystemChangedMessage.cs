using SkEditor.Workspace.Filesystem.Abstractions;
using SkEditor.Workspace.UI.Tree;

namespace SkEditor.Workspace.Projects.Messages;

public record FileSystemChangedMessage(FileTreeViewModel Sender, IFileSystem? NewFileSystem);