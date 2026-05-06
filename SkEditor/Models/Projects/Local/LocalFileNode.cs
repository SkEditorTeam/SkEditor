using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Models.Files;

namespace SkEditor.Models.Projects.Local;

public partial class LocalFileNode(string name, string path) : BaseFileNode(name, path)
{
    public override IRelayCommand OpenCommand => OpenFileCommand;

    public override bool CanOpen => true;
    public override bool CanOpenInExplorer => true;

    [RelayCommand]
    public void OpenFile()
    {
        WeakReferenceMessenger.Default.Send(new OpenFileMessage(new LocalFile(Path, Name)));
    }
}