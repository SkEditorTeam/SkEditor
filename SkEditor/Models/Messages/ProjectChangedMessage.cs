using SkEditor.Contracts.Models;

namespace SkEditor.Models.Messages;

public record ProjectChangedMessage(IFileSystem? NewFileSystem);