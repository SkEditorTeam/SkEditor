using System.IO;
using R3;

namespace SkEditor.Utilities.Projects;

public static class FileWatcherExtensions
{
    extension(FileSystemWatcher watcher)
    {
        public Observable<ErrorEventArgs> ErrorAsObservable()
        {
            return Observable.FromEvent<ErrorEventHandler, ErrorEventArgs>(
                handler => (_, e) => handler(e),
                h => watcher.Error += h,
                h => watcher.Error -= h
            );
        }

        public Observable<FileSystemEventArgs> CreatedAsObservable()
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                handler => (_, e) => handler(e),
                h => watcher.Created += h,
                h => watcher.Created -= h
            );
        }

        public Observable<FileSystemEventArgs> DeletedAsObservable()
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                handler => (_, e) => handler(e),
                h => watcher.Deleted += h,
                h => watcher.Deleted -= h
            );
        }

        public Observable<RenamedEventArgs> RenamedAsObservable()
        {
            return Observable.FromEvent<RenamedEventHandler, RenamedEventArgs>(
                handler => (_, e) => handler(e),
                h => watcher.Renamed += h,
                h => watcher.Renamed -= h
            );
        }

        public Observable<FileSystemEventArgs> ChangedAsObservable()
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                handler => (_, e) => handler(e),
                h => watcher.Changed += h,
                h => watcher.Changed -= h
            );
        }
    }
}