using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SkEditor.Views;

namespace SkEditor.API;

public class Windows : IWindows
{
    private readonly Queue<Func<Task>> _dialogQueue = new();
    private readonly SemaphoreSlim _dialogSemaphore = new(1, 1);
    private volatile bool _isProcessingQueue;

    public MainWindow? GetMainWindow()
    {
        return (MainWindow?)(Application.Current?.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)
            ?.MainWindow;
    }

    public Window? GetCurrentWindow()
    {
        IReadOnlyList<Window>? windows =
            (Application.Current?.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)?.Windows;
        return windows is { Count: > 0 } ? windows[^1] : GetMainWindow();
    }

    private async Task WaitForMainWindow()
    {
        const int maxWaitTime = 10000;
        const int checkInterval = 100;
        int waitedTime = 0;

        while (waitedTime < maxWaitTime)
        {
            var currentWindow = GetCurrentWindow();

            if (currentWindow != null && currentWindow is not SplashScreen)
            {
                return;
            }

            await Task.Delay(checkInterval);
            waitedTime += checkInterval;
        }
    }

    private async Task ProcessDialogQueue()
    {
        await _dialogSemaphore.WaitAsync();

        try
        {
            if (_isProcessingQueue) return;
            _isProcessingQueue = true;

            while (true)
            {
                Func<Task>? dialogTask;

                lock (_dialogQueue)
                {
                    if (_dialogQueue.Count == 0) break;
                    dialogTask = _dialogQueue.Dequeue();
                }

                if (dialogTask != null)
                {
                    await dialogTask();
                }
            }
        }
        finally
        {
            _isProcessingQueue = false;
            _dialogSemaphore.Release();
        }
    }

    private void EnqueueDialogFireAndForget(Func<Task> dialogFunc)
    {
        lock (_dialogQueue)
        {
            _dialogQueue.Enqueue(async () =>
            {
                try
                {
                    await WaitForMainWindow();

                    await Dispatcher.UIThread.InvokeAsync(async () => { await dialogFunc(); });
                }
                catch (Exception ex)
                {
                    SkEditorAPI.Logs.Error($"Error in dialog queue processing: {ex.Message}");
                }
            });
        }

        _ = Task.Run(ProcessDialogQueue);
    }

    private async Task<T> EnqueueDialog<T>(Func<Task<T>> dialogFunc)
    {
        var tcs = new TaskCompletionSource<T>();

        lock (_dialogQueue)
        {
            _dialogQueue.Enqueue(async () =>
            {
                try
                {
                    await WaitForMainWindow();

                    var result = await Dispatcher.UIThread.InvokeAsync(async () => await dialogFunc());

                    tcs.SetResult(result);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });
        }

        _ = Task.Run(ProcessDialogQueue);

        return await tcs.Task;
    }

    public async Task<ContentDialogResult> ShowDialog(string title,
        string message,
        object? icon = null,
        string? cancelButtonText = null,
        string primaryButtonText = "Okay", bool translate = true)
    {
        return await EnqueueDialog(async () =>
            await ShowDialogInternal(title, message, icon, cancelButtonText, primaryButtonText, translate));
    }

    public Task ShowMessage(string title, string message)
    {
        EnqueueDialogFireAndForget(async () => { await ShowDialogInternal(title, message, Symbol.FlagFilled); });
        return Task.CompletedTask;
    }

    public Task ShowError(string error)
    {
        EnqueueDialogFireAndForget(async () =>
        {
            await ShowDialogInternal(Translation.Get("Error"), error, Symbol.AlertFilled);
        });
        return Task.CompletedTask;
    }

    private async Task<ContentDialogResult> ShowDialogInternal(string title,
        string message,
        object? icon = null,
        string? cancelButtonText = null,
        string primaryButtonText = "Okay", bool translate = true)
    {
        object? background = null;
        Application.Current?.TryGetResource("MessageBoxBackground", out background);
        ContentDialog dialog = new()
        {
            Title = translate ? TryGetTranslation(title) : title,
            Background = background as ImmutableSolidColorBrush,
            PrimaryButtonText = translate ? TryGetTranslation(primaryButtonText) : primaryButtonText,
            CloseButtonText = translate ? TryGetTranslation(cancelButtonText) : cancelButtonText
        };

        icon = icon switch
        {
            Symbol symbol => new SymbolIconSource { Symbol = symbol, FontSize = 40 },
            _ => icon
        };

        IconSource? source = icon switch
        {
            IconSource iconSource => iconSource,
            null => null,
            _ => throw new ArgumentException("Icon must be of type IconSource, Symbol or SymbolIconSource.")
        };

        switch (source)
        {
            case FontIconSource fontIconSource:
                fontIconSource.FontSize = 40;
                break;
            case SymbolIconSource symbolIconSource:
                symbolIconSource.FontSize = 40;
                break;
        }

        IconSourceElement iconElement = new()
        {
            IconSource = source,
            MinWidth = 40,
            MinHeight = 40,
        };

        Grid grid = new() { ColumnDefinitions = new ColumnDefinitions("Auto,*") };

        double iconMargin = iconElement.IconSource is not null ? 24 : 0;

        TextBlock textBlock = new()
        {
            Text = TryGetTranslation(message),
            FontSize = 16,
            Margin = new Thickness(Math.Max(10, iconMargin), 0, 0, 0),
            VerticalAlignment = VerticalAlignment.Center,
            TextWrapping = TextWrapping.Wrap,
            MaxWidth = 400
        };

        Grid.SetColumn(iconElement, 0);
        Grid.SetColumn(textBlock, 1);

        if (iconElement.IconSource is not null)
        {
            grid.Children.Add(iconElement);
        }

        grid.Children.Add(textBlock);

        dialog.Content = grid;

        return await dialog.ShowAsync(GetCurrentWindow());

        static string? TryGetTranslation(string? input)
        {
            if (input == null)
            {
                return null;
            }

            string translation = Translation.Get(input);
            return translation == input ? input : translation;
        }
    }

    public async Task<string?> AskForFile(FilePickerOpenOptions options)
    {
        await WaitForMainWindow();

        return await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            Window? topLevel = GetCurrentWindow();
            if (topLevel is null) return null;
            IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
            return files.Count == 0 ? null : files[0]?.Path.AbsolutePath;
        });
    }

    public void ShowWindow(Window window)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Window? topLevel = GetCurrentWindow();
            if (topLevel is not null)
            {
                window.Show(topLevel);
            }
        });
    }

    public async Task ShowWindowAsDialog(Window window)
    {
        await WaitForMainWindow();

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            Window? topLevel = GetCurrentWindow();
            if (topLevel is not null)
            {
                await window.ShowDialog(topLevel);
            }
        });
    }
}