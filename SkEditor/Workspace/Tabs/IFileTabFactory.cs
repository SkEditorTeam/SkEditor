using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Session.Models;
using FileTabViewModel = SkEditor.Editor.UI.Tabs.FileTabViewModel;

namespace SkEditor.Workspace.Tabs;

public interface IFileTabFactory
{
    FileTabViewModel CreateFromFile(BaseFile file);
    FileTabViewModel CreateFromSession(SessionStateFile file);
    FileTabViewModel CreateNewFile(string name);
}