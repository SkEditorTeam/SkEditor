using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Platform.Storage;
using FluentAvalonia.UI.Controls;
using SkEditor.Utilities;
using SkEditor.Views;

namespace SkEditor.API;

public class Windows : IWindows
{
    public MainWindow? GetMainWindow()
    {
        return (MainWindow?)(Application.Current?.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)?.MainWindow;
    }

    public Window? GetCurrentWindow()
    {
        IReadOnlyList<Window>? windows =
            (Application.Current?.ApplicationLifetime as ClassicDesktopStyleApplicationLifetime)?.Windows;
        return windows is { Count: > 0 } ? windows[^1] : GetMainWindow();
    }

    public async Task<ContentDialogResult> ShowDialog(string title,
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
            IconSource iconSource => iconSource,
            Symbol symbol => new SymbolIconSource { Symbol = symbol, FontSize = 40 },
            _ => icon
        };

        IconSource? source = null;
        if (icon is not IconSource)
        {
            if (icon is null)
            {
                source = null;
            }
            else
            {
                throw new ArgumentException("Icon must be of type IconSource, Symbol or SymbolIconSource.");
            }
        }

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
            Height = 40,
            Width = 40
        };

        Grid grid = new() { ColumnDefinitions = new ColumnDefinitions("Auto,*") };

        double iconMargin = iconElement.IconSource is not null ? 24 : 0;

        TextBlock textBlock = new()
        {
            Text = TryGetTranslation(message),
            FontSize = 16,
            Margin = new Thickness(Math.Max(10, iconMargin), 10, 0, 0),
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

    public async Task ShowMessage(string title, string message)
    {
        await ShowDialog(title, message, Symbol.FlagFilled);
    }

    public async Task ShowError(string error)
    {
        await ShowDialog(Translation.Get("Error"), error, Symbol.AlertFilled);
    }

    public async Task<string?> AskForFile(FilePickerOpenOptions options)
    {
        Window? topLevel = GetCurrentWindow();
        if (topLevel is null) return null;
        IReadOnlyList<IStorageFile> files = await topLevel.StorageProvider.OpenFilePickerAsync(options);
        return files.Count == 0 ? null : files[0]?.Path.AbsolutePath;
    }

    public void ShowWindow(Window window)
    {
        Window? topLevel = GetCurrentWindow();
        if (topLevel is null) return;
        window.Show(topLevel);
    }

    public Task ShowWindowAsDialog(Window window)
    {
        Window? topLevel = GetCurrentWindow();
        return topLevel is null ? Task.CompletedTask : window.ShowDialog(topLevel);
    }
}