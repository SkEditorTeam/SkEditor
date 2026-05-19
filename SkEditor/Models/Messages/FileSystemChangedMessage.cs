using SkEditor.Contracts.Models;
using SkEditor.ViewModels.Controls;

namespace SkEditor.Models.Messages;

public record FileSystemChangedMessage(FileTreeViewModel Sender, IFileSystem? NewFileSystem);