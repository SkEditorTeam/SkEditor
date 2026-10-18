using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FluentIcons.Common;
using SkEditor.Appearance.Icons;
using SkEditor.Core.Localization;
using SkEditor.Layout.Sidebar.Messages;
using SkEditor.Workspace.Projects;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.UI.Tree;

namespace SkEditor.Layout.Sidebar.UI.Panes;

public partial class ExplorerViewModel : ObservableObject, ISidebarPanelViewModel,
    IRecipient<OpenExplorerFolderMessage>, IRecipient<ProjectChangedMessage>
{
    private readonly IMessenger _messenger;
    private readonly IProjectService _projects;

    public required FileTreeViewModel FileTree { get; init; }
    public StandardIcon Icon => Symbol.Folder;

    public TranslatableString Name => TranslationKey.LocalExplorerTitle;

    public ExplorerViewModel(FileTreeViewModel fileTree,
        IProjectService projects,
        IMessenger messenger)
    {
        _projects = projects;
        FileTree = fileTree;

        _messenger = messenger;
        _messenger.Register<OpenExplorerFolderMessage>(this);
        _messenger.Register<ProjectChangedMessage>(this);
    }

    public void Receive(OpenExplorerFolderMessage message)
    {
        _ = HandleOpenExplorerFolderMessageAsync(message);
    }

    public void Receive(ProjectChangedMessage message)
    {
        FileTree.FileSystem = message.NewFileSystem;

        if (message.NewFileSystem is null)
        {
            _projects.CloseProject();
            return;
        }

        _ = FileTree.LoadFileSystemAsync();
    }

    [RelayCommand]
    public async Task<bool> OpenFolder()
    {
        return await _projects.OpenProjectAsync();
    }

    private async Task HandleOpenExplorerFolderMessageAsync(
        OpenExplorerFolderMessage m)
    {
        if (m.FolderPath is not null)
        {
            _projects.OpenProject(m.FolderPath);
        }
        else
        {
            bool success = await OpenFolder();
            if (!success)
            {
                return;
            }
        }

        _messenger.Send(new RequestPaneOpenMessage(Name.Value));
    }
}