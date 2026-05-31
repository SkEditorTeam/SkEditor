using System;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Features;

public class AutoSaveFeature(AppSettings appSettings) : IEditorFeature
{
    private readonly TimeSpan _debounceDelay = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _maxDelay = TimeSpan.FromSeconds(10);
    private CancellationTokenSource? _cts;
    private DateTime _lastChangeAt = DateTime.MinValue;
    private FileTabViewModel? _tab;

    public bool CanAttach(FileTabViewModel tab)
    {
        return appSettings.IsAutoSaveEnabled;
    }

    public void Attach(FileTabViewModel tab)
    {
        _tab = tab;

        tab.Document.TextChanged += OnTextChanged;
        tab.ClosingAsync += OnClosingAsync;
    }

    public void Detach(FileTabViewModel tab)
    {
        _tab = null;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        tab.Document.TextChanged -= OnTextChanged;
        tab.ClosingAsync -= OnClosingAsync;
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (_tab?.File.Path is null)
        {
            return;
        }

        if (_lastChangeAt == DateTime.MinValue)
        {
            _lastChangeAt = DateTime.UtcNow;
        }

        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        TimeSpan elapsed = DateTime.UtcNow - _lastChangeAt;
        TimeSpan remainingDelay = elapsed >= _maxDelay
            ? TimeSpan.Zero
            : _debounceDelay;

        _ = SaveAfterDelayAsync(remainingDelay, _cts.Token);
    }

    private async Task SaveAfterDelayAsync(TimeSpan delay, CancellationToken token)
    {
        if (_tab is null)
        {
            return;
        }

        try
        {
            await Task.Delay(delay, token);

            await _tab.SaveAsync();

            if (!token.IsCancellationRequested)
            {
                _lastChangeAt = DateTime.MinValue;
            }

            _lastChangeAt = DateTime.MinValue;
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    private async Task OnClosingAsync()
    {
        if (_cts is not null && !_cts.IsCancellationRequested)
        {
            await _cts.CancelAsync();

            await SaveAfterDelayAsync(TimeSpan.Zero, CancellationToken.None);
        }
    }
}