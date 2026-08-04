using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SkEditor.Contracts.Services;
using SkEditor.Models.Logging;

namespace SkEditor.Analyzing.Processes;

public class AnalyzerProcessRunner(ILogService logs) : IAnalyzerProcessRunner
{
    private Process? _process;

    public bool IsRunning => _process is { HasExited: false };

    public event EventHandler<string>? OutputDataReceived;
    public event EventHandler<string>? ErrorDataReceived;
    public event EventHandler? ProcessExited;

    public void Start(ProcessStartInfo startInfo)
    {
        try
        {
            if (IsRunning)
            {
                Stop();
            }

            _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };

            _process.OutputDataReceived += (_, args) =>
            {
                if (args.Data != null) OutputDataReceived?.Invoke(this, args.Data);
            };

            _process.ErrorDataReceived += (_, args) =>
            {
                if (args.Data != null) ErrorDataReceived?.Invoke(this, args.Data);
            };

            _process.Exited += (_, _) => ProcessExited?.Invoke(this, EventArgs.Empty);

            _process.Start();
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Failed to start Analyzer process.", LogSource.Analyzer);
            throw;
        }
    }

    public async Task WriteInputLineAsync(string input)
    {
        if (_process is null || _process.HasExited)
        {
            throw new InvalidOperationException(
                $"Process is not running. is null: {_process is null}, has exited: {_process?.HasExited}");
        }

        await _process.StandardInput.WriteLineAsync(input);
    }

    public void Stop()
    {
        if (_process is null) return;

        try
        {
            if (!_process.HasExited)
            {
                _process.Kill(true);
            }

            _process.Dispose();
        }
        catch (Exception ex)
        {
            logs.Error(ex, "Error while stopping Analyzer process.", LogSource.Analyzer);
        }
        finally
        {
            _process = null;
        }
    }

    public void Dispose()
    {
        Stop();
        GC.SuppressFinalize(this);
    }
}