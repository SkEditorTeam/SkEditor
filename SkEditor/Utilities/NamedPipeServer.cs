using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Threading;
using Serilog;
using SkEditor.API;
using SkEditor.Utilities.Files;
using MainWindow = SkEditor.Views.Windows.MainWindow;

namespace SkEditor.Utilities;

public static class NamedPipeServer
{
    private static bool _isRunning;

    public static void Start()
    {
        if (_isRunning)
        {
            return;
        }

        _isRunning = true;

        Task.Run(RunServer);
    }

    private static async Task RunServer()
    {
        try
        {
            while (true)
            {
                await using NamedPipeServerStream serverStream = new("SkEditor", PipeDirection.InOut, 1,
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

    private static async Task OnClientConnected(NamedPipeServerStream namedPipeServerStream)
    {
        try
        {
            byte[] buffer = new byte[1024];
            int bytesRead = await namedPipeServerStream.ReadAsync(buffer.AsMemory(0, 1024));

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (bytesRead is not (0 or 1))
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    OpenFiles(message);
                }

                BringMainWindowToFront();
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error in named pipe communication");
        }
    }


    private static void OpenFiles(string filesToOpen)
    {
        filesToOpen.Split(Environment.NewLine)
            .Where(x => !string.IsNullOrEmpty(x)).ToList().ForEach(FileHandler.OpenFile);
    }

    private static void BringMainWindowToFront()
    {
        MainWindow? mainWindow = SkEditorAPI.Windows.GetMainWindow();
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