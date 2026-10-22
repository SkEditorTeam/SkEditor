using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using ObservableCollections;
using R3;
using SkEditor.Analyzing.Models;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Localization;
using SkEditor.Core.System;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Editor.UI.Tabs;

public partial class FileTabViewModel(
    TranslatableString header,
    AppSettings appSettings,
    ExperimentsConfig experiments,
    ILinkService links,
    IClipboardService clipboard)
    : TabViewModelBase(header, Symbol.Document), ITextEditorTab, IFocusableTab, IFeatureTab,
        IFileTab, ISaveStateTab, IDiagnosticsTab
{
    public AppSettings AppSettings { get; } = appSettings;

    [ObservableProperty]
    public partial int CaretOffset { get; set; } = 0;

    public IRelayCommand CopyNameRelayCommand => CopyNameCommand;
    public IRelayCommand CopyPathRelayCommand => CopyPathCommand;
    public Dictionary<int, AnalyzerError> DiagnosticDictionary { get; } = [];

    public ObservableList<AnalyzerError> Diagnostics { get; } = [];
    public TextDocument Document { get; } = new();
    public ExperimentsConfig Experiments { get; } = experiments;
    public required BaseFile File { get; init; }

    [ObservableProperty]
    public partial double FontSize { get; set; } = appSettings.EditorFontSize;

    public override bool IsCacheable => true;

    public IRelayCommand OpenInExplorerRelayCommand => OpenInExplorerCommand;
    public string? Path => File.Path;

    [ObservableProperty]
    public partial Vector ScrollOffset { get; set; } = Vector.Zero;

    public override string? ToolTip => Path;

    public BindableReactiveProperty<bool> WordWrap { get; } = appSettings
        .ObservePropertyChanged(x => x.IsWordWrapEnabled)
        .ToBindableReactiveProperty(appSettings.IsWordWrapEnabled);

    public async Task UpdateDocumentText(string text)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Document.Text = text;
            Document.UndoStack.ClearAll();
            MarkOriginal();
        });
    }

    public void MarkOriginal()
    {
        Document.UndoStack.MarkAsOriginalFile();
    }

    public void WriteContentStream(StreamWriter writer)
    {
        ITextSource? snapshot = Document.CreateSnapshot();
        snapshot.WriteTextTo(writer);
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
    
    public event Func<Task>? ClosingAsync;

    public async Task<bool> CanCloseAsync()
    {
        if (ClosingAsync is null)
        {
            return true;
        }

        foreach (Func<Task> handler in ClosingAsync.GetInvocationList().Cast<Func<Task>>())
        {
            await handler();
        }

        return true;
    }

    public void OnClosed()
    {
    }
}