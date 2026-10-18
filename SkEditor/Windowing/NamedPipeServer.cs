using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Serilog;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Types;
using MainWindow = SkEditor.Windowing.UI.MainWindow;

namespace SkEditor.Windowing;

public class NamedPipeServer(WindowRegistry windowRegistry, IFileService fileService)
{
    public const string PipeName = "SkEditor3";

    private bool _isRunning;

    public void Start()
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;

        Task.Run(RunServer);
    }

    private async Task RunServer()
    {
        try
        {
            while (true)
            {
                await using NamedPipeServerStream serverStream = new(PipeName, PipeDirection.InOut, 1,
                    PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await serverStream.WaitForConnectionAsync();

                await Task.Run(() => OnClientConnected(serverStream));
            }
        }
        catch (Exception ex)
        {
            Log.Error("Named Pipe Server error: {ExMessage}\n\n{ExStackTrace}", ex.Message, ex.StackTrace);
        }
    }

    private async Task OnClientConnected(NamedPipeServerStream namedPipeServerStream)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await namedPipeServerStream.ReadAsync(buffer.AsMemory(0, 1024));

            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                if (bytesRead is not (0 or 1))
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    await OpenFiles(message);
                }

                BringMainWindowToFront();
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error in named pipe communication");
        }
    }


    private async Task OpenFiles(string filesToOpen)
    {
        IEnumerable<string> files = filesToOpen.Split(Environment.NewLine)
            .Where(x => !string.IsNullOrEmpty(x));

        foreach (string file in files)
        {
            await fileService.OpenFileAsync(new LocalFile(file));
        }
    }

    private void BringMainWindowToFront()
    {
        MainWindow? mainWindow = windowRegistry.Active;
        if (mainWindow == null)
        {
            return;
        }

        if (mainWindow.WindowState == WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
        }

        mainWindow.Topmost = true;
        mainWindow.Activate();

        Dispatcher.UIThread.InvokeAsync(() => mainWindow.Topmost = false);
    }
}