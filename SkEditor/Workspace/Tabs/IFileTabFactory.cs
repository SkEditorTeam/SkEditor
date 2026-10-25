using SkEditor.Editor.UI.Tabs;
using SkEditor.Windowing.Errors.Models;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Session.Models;

namespace SkEditor.Workspace.Tabs;

public interface IFileTabFactory
{
    FileTabViewModel CreateFromFile(BaseFile file);
    FileTabViewModel CreateFromSession(SessionStateFile file);
    FileTabViewModel CreateFromCrash(CrashStateFile file);
    FileTabViewModel CreateNewFile(string name);
}