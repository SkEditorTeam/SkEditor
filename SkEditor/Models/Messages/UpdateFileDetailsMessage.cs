namespace SkEditor.Models.Messages;

public record UpdateFileDetailsMessage(string OldPath, string NewPath, string NewName);