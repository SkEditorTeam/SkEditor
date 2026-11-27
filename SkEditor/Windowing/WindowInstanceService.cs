using System;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using SkEditor.Core.Logging;

namespace SkEditor.Windowing;

public class WindowInstanceService(ILogService logs)
{
    private Mutex? _mutex;
    
    public async Task<bool> HandleSingleInstance(IClassicDesktopStyleApplicationLifetime desktop)
    {
        _mutex = new Mutex(true, $@"Local\{NamedPipeServer.PipeName}Mutex");

        bool isFirstInstance;
        try
        {
            isFirstInstance = _mutex.WaitOne(TimeSpan.Zero, true);
        }
        catch (AbandonedMutexException ex)
        {
            logs.Error(ex, "Abandoned mutex");
            isFirstInstance = true;
        }

        if (isFirstInstance)
        {
            desktop.Exit += (_, _) =>
            {
                ReleaseMutex();
            };

            return true;
        }

        await SendArgsToExistingInstance();
        desktop.Shutdown();
        return false;
    }

    public void ReleaseMutex()
    {
        try
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
            _mutex = null;
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Error releasing mutex");
        }
    }
    
    private async Task SendArgsToExistingInstance()
    {
        try
        {
            string[] args = Environment.GetCommandLineArgs();
            await using NamedPipeClientStream namedPipeClientStream = new(NamedPipeServer.PipeName);
            await namedPipeClientStream.ConnectAsync();
            if (args is { Length: > 1 })
            {
                byte[] buffer = Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, args.Skip(1)));
                namedPipeClientStream.Write(buffer, 0, buffer.Length);
            }
            else
            {
                namedPipeClientStream.WriteByte(0);
            }
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Error connecting to named pipe");
        }
    }
}