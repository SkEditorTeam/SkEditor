namespace SkEditor.Workspace.Files.Messages;

public record UpdateFileDetailsMessage(string OldPath, string NewPath, string NewName);