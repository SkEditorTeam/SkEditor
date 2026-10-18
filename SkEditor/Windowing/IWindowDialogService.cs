using System.Threading.Tasks;

namespace SkEditor.Windowing;

public interface IWindowDialogService
{
    Task ShowPublishDialogAsync();
    Task ShowGoToLineDialogAsync();
    Task ShowAboutDialogAsync();
    Task ShowManageRemoteConnectionsDialogAsync();
}