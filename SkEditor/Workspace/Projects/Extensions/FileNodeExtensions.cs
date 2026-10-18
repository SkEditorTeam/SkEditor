using R3;
using SkEditor.Models.Projects.Types;

namespace SkEditor.Utilities.Extensions;

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