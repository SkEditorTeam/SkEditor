using System.Collections.Generic;
using Avalonia.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Messaging;
using SkEditor.Windowing.UI;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Projects.Messages;
using SkEditor.Workspace.Tabs;

namespace SkEditor.Windowing;

public class DragDropService(IFileTabService fileTabs)
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
                    _ = fileTabs.OpenFileAsync(new LocalFile(file.Path.LocalPath));
                    break;
                case IStorageFolder folder:
                    WeakReferenceMessenger.Default.Send(new OpenExplorerFolderMessage(folder.Path.LocalPath));
                    break;
            }
        }

        args.Handled = true;
    }
}