using System.IO;
using System.IO.Abstractions;
using R3;

namespace SkEditor.Core.Extensions;

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

    extension(IFileSystemWatcher fileSystemWatcher)
    {
        public Observable<ErrorEventArgs> ErrorAsObservable()
        {
            return Observable.FromEvent<ErrorEventHandler, ErrorEventArgs>(
                handler => (_, e) => handler(e),
                h => fileSystemWatcher.Error += h,
                h => fileSystemWatcher.Error -= h
            );
        }

        public Observable<FileSystemEventArgs> CreatedAsObservable()
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                handler => (_, e) => handler(e),
                h => fileSystemWatcher.Created += h,
                h => fileSystemWatcher.Created -= h
            );
        }

        public Observable<FileSystemEventArgs> DeletedAsObservable()
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                handler => (_, e) => handler(e),
                h => fileSystemWatcher.Deleted += h,
                h => fileSystemWatcher.Deleted -= h
            );
        }

        public Observable<RenamedEventArgs> RenamedAsObservable()
        {
            return Observable.FromEvent<RenamedEventHandler, RenamedEventArgs>(
                handler => (_, e) => handler(e),
                h => fileSystemWatcher.Renamed += h,
                h => fileSystemWatcher.Renamed -= h
            );
        }

        public Observable<FileSystemEventArgs> ChangedAsObservable()
        {
            return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(
                handler => (_, e) => handler(e),
                h => fileSystemWatcher.Changed += h,
                h => fileSystemWatcher.Changed -= h
            );
        }
    }
}