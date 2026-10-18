using SkEditor.Workspace.Filesystem.Abstractions;

namespace SkEditor.Workspace.Projects.Messages;

public record ProjectChangedMessage(IFileSystem? NewFileSystem);