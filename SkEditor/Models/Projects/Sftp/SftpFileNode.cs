using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Renci.SshNet;
using SkEditor.Contracts.Services;

namespace SkEditor.Models.Projects.Sftp;

public partial class SftpFileNode(string name, string path, SftpClient client) : BaseFileNode(name, path)
{
    public override IRelayCommand OpenCommand => OpenFileCommand;

    [RelayCommand]
    public void OpenFile()
    {
        App.Services.GetRequiredService<INotificationService>().Show("Opened", "This file has been opened.");
    }
}