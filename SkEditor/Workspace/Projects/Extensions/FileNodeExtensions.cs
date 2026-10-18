using R3;
using SkEditor.Workspace.Filesystem.Abstractions;

namespace SkEditor.Workspace.Projects.Extensions;

public static class FileNodeExtensions
{
    extension(FileSystemNode node)
    {
        public Observable<NodeRenamedEventArgs> RenamedAsObservable()
        {
            return Observable.FromEventHandler<NodeRenamedEventArgs>(
                    h => node.Renamed += h,
                    h => node.Renamed -= h
                )
                .Select(x => x.e);
        }
    }
}