using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using ObservableCollections;
using R3;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Models.Analyzer;
using SkEditor.Models.Dialogs;
using SkEditor.Models.Files;
using SkEditor.Models.Notifications;
using SkEditor.Utilities.Extensions;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.ViewModels.Tabs;

public partial class FileTabViewModel(
    TranslatableString header,
    AppSettings appSettings,
    ExperimentsConfig experiments,
    IDialogService dialogs,
    INotificationService notifications,
    IFilePickerService filePicker,
    IFileFeatureService features,
    IRecentFileService recentFiles,
    ILinkService links,
    IClipboardService clipboard,
    ILogService logs)
    : TabViewModelBase(header, Symbol.Document), ITextEditorTab, IFocusableTab, IFeatureTab, ISaveableTab,
        IFileTab, IClosableTab, ISaveStateTab, IDiagnosticsTab
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(false);

    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    private CancellationTokenSource? _saveCts;

    public AppSettings AppSettings { get; } = appSettings;

    [ObservableProperty]
    public partial int CaretOffset { get; set; } = 0;

    public override IRelayCommand CloseCommand => CloseTabCommand;
    public Dictionary<int, AnalyzerError> DiagnosticDictionary { get; } = [];

    public ObservableList<AnalyzerError> Diagnostics { get; } = [];
    public TextDocument Document { get; } = new();
    public ExperimentsConfig Experiments { get; } = experiments;
    public required BaseFile File { get; init; }

    public override string? ToolTip => Path;

    [ObservableProperty]
    public partial double FontSize { get; set; } = appSettings.EditorFontSize;

    public override bool IsCacheable => true;
    public string? Path => File.Path;
    public IRelayCommand OpenInExplorerRelayCommand => OpenInExplorerCommand;
    public IRelayCommand CopyNameRelayCommand => CopyNameCommand;
    public IRelayCommand CopyPathRelayCommand => CopyPathCommand;

    [ObservableProperty]
    public partial Vector ScrollOffset { get; set; } = Vector.Zero;

    public BindableReactiveProperty<bool> WordWrap { get; } = appSettings
        .ObservePropertyChanged(x => x.IsWordWrapEnabled)
        .ToBindableReactiveProperty(appSettings.IsWordWrapEnabled);

    public async Task<bool> LoadContentAsync()
    {
        if (string.IsNullOrEmpty(File.Path))
        {
            return false;
        }

        await using Stream stream = await File.LoadAsync();

        if (stream.IsLikelyBinary())
        {
            Dialog dialog = Dialog
                .Create(TranslationKey.BinaryFileTitle)
                .WithMessage(TranslationKey.BinaryFileFound)
                .WithIcon(Symbol.DocumentCube)
                .WithButtons(
                    new DialogButton(TranslationKey.ButtonOpen, DialogResult.Yes),
                    new DialogButton(TranslationKey.ButtonCancel, DialogResult.Cancel)
                );

            DialogResult result = await dialogs.ShowDialogAsync(dialog);

            if (result == StandardDialogResult.Cancel)
            {
                return false;
            }
        }

        using StreamReader reader = new(stream);

        string text = await reader.ReadToEndAsync();
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Document.Text = text;
            Document.UndoStack.ClearAll();
            Document.UndoStack.MarkAsOriginalFile();
        });
        return true;
    }

    /// <summary>
    ///     Saves the file. If the file lacks a path, it will do a "Save As" instead
    /// </summary>
    /// <returns>True if success, false otherwise (cancelled, failed)</returns>
    public async Task<bool> SaveAsync()
    {
        if (File.Path == null)
        {
            return await SaveAsAsync();
        }

        return await PerformSaveAsync();
    }

    /// <summary>
    ///     Prompts the user to select a save location and saves the file there. Updates the tab's header and path accordingly
    /// </summary>
    /// <returns>True if success, false otherwise (cancelled, failed)</returns>
    public async Task<bool> SaveAsAsync()
    {
        string? oldPath = File.Path;

        string? path = await filePicker.SaveFileAsync(Header.Value, oldPath);
        if (path == null)
        {
            return false;
        }

        File.Path = path;
        UpdateHeader();
        features.UpdateFeatures(this);
        recentFiles.UpdateRecentItem(oldPath, path);

        return await PerformSaveAsync();
    }

    public void UpdateHeader()
    {
        if (File.Path is null)
        {
            return;
        }

        string fileName = System.IO.Path.GetFileName(File.Path);
        Header = TranslatableString.FromRawString(fileName);
    }

    public string? GetExtension()
    {
        return System.IO.Path.GetExtension(File.Path ?? null)?.ToLowerInvariant();
    }

    [RelayCommand]
    public void OnFocus()
    {
        RequestFocus();
    }

    /// <summary>
    ///     Performs the actual save operation
    /// </summary>
    /// <returns>True if success, false otherwise (failed)</returns>
    private async Task<bool> PerformSaveAsync()
    {
        if (_saveCts is not null)
        {
            await _saveCts.CancelAsync();
        }

        _saveCts = new CancellationTokenSource();
        CancellationToken token = _saveCts.Token;

        using MemoryStream memoryStream = new();
        await using StreamWriter writer = new(memoryStream, Utf8NoBom, leaveOpen: true);

        ITextSource? snapshot = Document.CreateSnapshot();
        snapshot.WriteTextTo(writer);

        await writer.FlushAsync(token);
        memoryStream.Position = 0;

        await _saveSemaphore.WaitAsync(token);
        try
        {
            await File.SaveAsync(memoryStream, token);
        }
        catch (UnauthorizedAccessException ex)
        {
            logs.Error(ex, "Failed to save file due to unauthorized access.");
            Notification notification = Notification
                .Create(TranslationKey.FileExplorerSaveFailedTitle)
                .WithMessage(TranslationKey.FileExplorerSaveFailedPermission)
                .WithType(NotificationType.Error);

            notifications.Show(notification);
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
        finally
        {
            _saveSemaphore.Release();
        }

        Document.UndoStack.MarkAsOriginalFile();
        return true;
    }

    [RelayCommand]
    private async Task CloseTab()
    {
        if (ClosingAsync is not null)
        {
            foreach (Func<Task> handler in ClosingAsync.GetInvocationList().Cast<Func<Task>>())
            {
                await handler();
            }
        }

        if (!IsSaved)
        {
            Dialog dialog = Dialog
                .Create(TranslationKey.UnsavedFileTitle)
                .WithMessage(new TranslatableString(TranslationKey.UnsavedFileMessage, Header))
                .WithIcon(Symbol.Save)
                .WithButtons(
                    new DialogButton(TranslationKey.ButtonSave, DialogResult.Yes, true),
                    new DialogButton(TranslationKey.ButtonDontSave, DialogResult.No),
                    DialogButton.Cancel
                );

            DialogResult result = await dialogs.ShowDialogAsync(dialog);

            if (result == StandardDialogResult.Cancel)
            {
                return;
            }

            if (result == StandardDialogResult.Yes)
            {
                bool saved = await SaveAsync();
                if (!saved)
                {
                    return;
                }
            }
        }

        RequestClose();
    }

    [RelayCommand]
    private void OpenInExplorer()
    {
        if (Path is not null)
        {
            links.OpenFolderWithFileSelection(Path);
        }
    }

    [RelayCommand]
    private async Task CopyName()
    {
        await clipboard.CopyTextAsync(Header.Value);
    }

    [RelayCommand]
    private async Task CopyPath()
    {
        if (Path is not null)
        {
            string pathToCopy = OperatingSystem.IsWindows() && AppSettings.IsExplorerForwardSlashesCopyEnabled
                ? Path.Replace('\\', '/')
                : Path;

            await clipboard.CopyTextAsync(pathToCopy);
        }
    }
    
    /// <summary>
    ///     Fired before the close logic begins. Allows async features (like AutoSave) to flush data.
    /// </summary>
    public event Func<Task>? ClosingAsync;
}