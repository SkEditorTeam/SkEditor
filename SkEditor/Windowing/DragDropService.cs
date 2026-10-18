using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Contracts.Services.Files;
using SkEditor.Models.Files;
using SkEditor.Models.Messages;
using SkEditor.Views.Windows;

namespace SkEditor.Services.Windowing;

public class DragDropService(IFileService files)
{
    public void AttachDragDropEvents(MainWindow mainWindow)
    {
        mainWindow.AddHandler(DragDrop.DropEvent, OnDragDrop);
    }

    private void OnDragDrop(object? __, DragEventArgs args)
    {
        if (!args.DataTransfer.Contains(DataFormat.File))
        {
            return;
        }

        IEnumerable<IDataTransferItem>? items = args.DataTransfer.GetItems(DataFormat.File);
        if (items is null)
        {
            return;
        }

        foreach (IDataTransferItem dataTransferItem in items)
        {
            IStorageItem? storageItem = dataTransferItem.TryGetFile();

            switch (storageItem)
            {
                case IStorageFile file:
                    _ = files.OpenFileAsync(new LocalFile(file.Path.LocalPath));
                    break;
                case IStorageFolder folder:
                    WeakReferenceMessenger.Default.Send(new OpenExplorerFolderMessage(folder.Path.LocalPath));
                    break;
            }
        }

        args.Handled = true;
    }
}