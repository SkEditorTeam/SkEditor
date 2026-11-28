using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Capabilities;

namespace SkEditor.Editor.UI.Tabs;

public sealed partial class FileTabViewModel : TabViewModelBase, ITextEditorTab, IFocusableTab, IFeatureTab,
    IFileTab, ISaveStateTab, IDiagnosticsTab, IDisposable
{
    private readonly SerialDisposable _fileSubscription = new();

    private readonly Subject<string?> _pathChanged = new();
    private DisposableBag _disposables;
    public AppSettings AppSettings { get; }

    [ObservableProperty]
    public partial int CaretOffset { get; set; } = 0;

    public Dictionary<int, AnalyzerError> DiagnosticDictionary { get; } = [];

    public ObservableList<AnalyzerError> Diagnostics { get; } = [];
    public TextDocument Document { get; } = new();
    public ExperimentsConfig Experiments { get; }
    public BaseFile File { get; private set; }

    [ObservableProperty]
    public partial double FontSize { get; set; }

    public override bool IsCacheable => true;

    public string? Path => File.Path;

    public Observable<string?> PathChanged => _pathChanged;

    [ObservableProperty]
    public partial Vector ScrollOffset { get; set; } = Vector.Zero;

    public override string? ToolTip => Path;

    public BindableReactiveProperty<bool> WordWrap { get; }

    public FileTabViewModel(TranslatableString header, BaseFile file,
        AppSettings appSettings,
        ExperimentsConfig experiments) : base(header, Symbol.Document)
    {
        AppSettings = appSettings;
        Experiments = experiments;
        FontSize = appSettings.EditorFontSize;
        WordWrap = appSettings
            .ObservePropertyChanged(x => x.IsWordWrapEnabled)
            .ToBindableReactiveProperty(appSettings.IsWordWrapEnabled);

        ChangeFile(file);
    }

    [MemberNotNull(nameof(File))]
    public void ChangeFile(BaseFile file)
    {
        File = file;
        _fileSubscription.Disposable = file.PathChanged.Subscribe(p => _pathChanged.OnNext(p));
        
        UpdateHeader();
        _pathChanged.OnNext(file.Path);
    }

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

    public void Dispose()
    {
        _fileSubscription.Dispose();
        _pathChanged.Dispose();
        WordWrap.Dispose();
        _disposables.Dispose();
    }

    public event Func<Task>? ClosingAsync;
}