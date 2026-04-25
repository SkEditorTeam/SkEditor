using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using SkEditor.Contracts.Services.Files;
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
            if (dataTransferItem.TryGetFile() is not IStorageFile file)
            {
                continue;
            }

            _ = files.OpenFileAsync(file.Path.LocalPath);
        }

        args.Handled = true;
    }
}