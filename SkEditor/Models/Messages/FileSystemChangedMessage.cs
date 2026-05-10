using SkEditor.Contracts.Models;

namespace SkEditor.Models.Messages;

public record FileSystemChangedMessage(IFileSystem? NewFileSystem);