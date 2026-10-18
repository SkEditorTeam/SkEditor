using SkEditor.Models.Projects.Sftp;
using SkEditor.Models.Remote;

namespace SkEditor.Models.Messages;

public record RemoteConnectionStateChangedMessage(RemoteConnection Connection, RemoteConnectionState State);