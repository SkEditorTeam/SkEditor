using System.Threading.Tasks;

namespace SkEditor.Contracts.Services.Windowing;

public interface IWindowDialogService
{
    Task ShowPublishDialogAsync();
    Task ShowGoToLineDialogAsync();
    Task ShowAboutDialogAsync();
}