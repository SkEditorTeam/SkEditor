using System;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Features;

public class AutoSaveFeature(AppSettings appSettings) : IEditorFeature
{
    private readonly TimeSpan _debounceDelay = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _maxDelay = TimeSpan.FromSeconds(10);
    private IClosableTab? _closableTab;
    private CancellationTokenSource? _cts;
    private IFileTab? _fileTab;
    private DateTime _lastChangeAt = DateTime.MinValue;
    private ISaveableTab? _saveableTab;
    private ITextEditorTab? _textEditorTab;

    public bool CanAttach(IFeatureTab tab)
    {
        if (!appSettings.IsAutoSaveEnabled)
        {
            return false;
        }

        return tab is ITextEditorTab and ISaveableTab and IClosableTab and IFileTab;
    }

    public void Attach(IFeatureTab tab)
    {
        if (tab is not (ITextEditorTab textEditorTab and ISaveableTab saveableTab and IClosableTab closableTab
            and IFileTab fileTab))
        {
            return;
        }

        _textEditorTab = textEditorTab;
        _saveableTab = saveableTab;
        _closableTab = closableTab;
        _fileTab = fileTab;

        _textEditorTab.Document.TextChanged += OnTextChanged;
        _closableTab.ClosingAsync += OnClosingAsync;
    }

    public void Detach(IFeatureTab tab)
    {
        if (_textEditorTab != null)
        {
            _textEditorTab.Document.TextChanged -= OnTextChanged;
        }

        if (_closableTab != null)
        {
            _closableTab.ClosingAsync -= OnClosingAsync;
        }

        _textEditorTab = null;
        _saveableTab = null;
        _closableTab = null;
        _fileTab = null;
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
    }

    private void OnTextChanged(object? sender, EventArgs e)
    {
        if (_fileTab?.Path is null)
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
        if (_saveableTab is null)
        {
            return;
        }

        try
        {
            await Task.Delay(delay, token);

            await _saveableTab.SaveAsync();

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