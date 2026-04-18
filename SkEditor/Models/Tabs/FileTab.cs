using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Models.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Models.Tabs;

public partial class FileTab(TranslatableString header, AppSettings appSettings, INotificationService notifications)
    : Tab(header)
{
    private readonly SemaphoreSlim _saveSemaphore = new(1, 1);

    private CancellationTokenSource? _saveCts;
    public required BaseFile File { get; init; }
    public TextDocument Document { get; } = new();

    public AppSettings AppSettings { get; } = appSettings;

    public override IRelayCommand CloseCommand => CloseTabCommand;

    [ObservableProperty]
    public partial double FontSize { get; set; } = appSettings.EditorFontSize;

    [ObservableProperty]
    public partial bool WordWrap { get; set; } = appSettings.IsWordWrapEnabled;

    [ObservableProperty]
    public partial Vector ScrollOffset { get; set; } = Vector.Zero;

    public async Task LoadContentAsync()
    {
        if (string.IsNullOrEmpty(File.Path))
        {
            return;
        }

        await using Stream stream = await File.LoadAsync();
        using StreamReader reader = new(stream);

        Document.Text = await reader.ReadToEndAsync();
        Document.UndoStack.ClearAll();
    }

    public async Task SaveContentAsync()
    {
        if (_saveCts is not null)
        {
            await _saveCts.CancelAsync();
        }

        _saveCts = new CancellationTokenSource();
        CancellationToken token = _saveCts.Token;

        using MemoryStream memoryStream = new();
        await using StreamWriter writer = new(memoryStream, Encoding.UTF8, leaveOpen: true);

        ITextSource? snapshot = Document.CreateSnapshot();
        snapshot.WriteTextTo(writer);

        await writer.FlushAsync(token);
        memoryStream.Position = 0;

        await _saveSemaphore.WaitAsync(token);
        try
        {
            await File.SaveAsync(memoryStream, token);
        }
        finally
        {
            _saveSemaphore.Release();
        }
    }

    [RelayCommand]
    private void CloseTab()
    {
        notifications.Debug("Close Editor Tab");

        RequestClose();
    }
}