using SkEditor.Workspace.Filesystem.Sftp;
using SkEditor.Workspace.Remote.Models;

namespace SkEditor.Workspace.Remote.Messages;

public record RemoteConnectionStateChangedMessage(RemoteConnection Connection, RemoteConnectionState State);