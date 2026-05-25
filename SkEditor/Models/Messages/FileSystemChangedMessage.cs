using SkEditor.Contracts.Models;
using SkEditor.ViewModels.Controls.FileTree;

namespace SkEditor.Models.Messages;

public record FileSystemChangedMessage(FileTreeViewModel Sender, IFileSystem? NewFileSystem);