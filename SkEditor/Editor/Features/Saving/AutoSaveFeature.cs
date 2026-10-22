using System;
using System.Threading;
using System.Threading.Tasks;
using SkEditor.Configuration.Configs;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Editor.Features.Saving;

public class AutoSaveFeature(AppSettings appSettings, IFileService files) : IEditorFeature
{
    private readonly TimeSpan _debounceDelay = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _maxDelay = TimeSpan.FromSeconds(10);
    private CancellationTokenSource? _cts;
    private IFileTab? _fileTab;
    private DateTime _lastChangeAt = DateTime.MinValue;
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
        if (tab is not (ITextEditorTab textEditorTab and IFileTab fileTab))
        {
            return;
        }

        _textEditorTab = textEditorTab;
        _fileTab = fileTab;

        _textEditorTab.Document.TextChanged += OnTextChanged;
        _fileTab.ClosingAsync += OnClosingAsync;
    }

    public void Detach(IFeatureTab tab)
    {
        if (_textEditorTab != null)
        {
            _textEditorTab.Document.TextChanged -= OnTextChanged;
        }

        if (_fileTab != null)
        {
            _fileTab.ClosingAsync -= OnClosingAsync;
        }

        _textEditorTab = null;
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
        if (_fileTab is null)
        {
            return;
        }

        try
        {
            await Task.Delay(delay, token);

            await files.SaveFileAsync(_fileTab);

            _lastChangeAt = DateTime.MinValue;
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    private async Task OnClosingAsync()
    {
        if (_fileTab is { IsSaved: false })
        {
            if (_cts is not null)
            {
                await _cts.CancelAsync();
            }

            await files.SaveFileAsync(_fileTab);
            _lastChangeAt = DateTime.MinValue;
        }
    }
}