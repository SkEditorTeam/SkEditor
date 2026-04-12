using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using SkEditor.Contracts.Services;
using SkEditor.Models.Files;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings.Configs;

namespace SkEditor.Models.Tabs;

public partial class FileTab(TranslatableString header, AppSettings appSettings, INotificationService notifications)
    : Tab(header), IDisposable
{
    public required BaseFile File { get; init; }
    public TextDocument Document { get; } = new();

    public AppSettings AppSettings { get; } = appSettings;

    public override IRelayCommand CloseCommand => CloseTabCommand;

    public void Dispose()
    {
        Document.TextChanged -= OnDocumentTextChanged;

        Document.UndoStack.ClearAll();

        GC.SuppressFinalize(this);
    }

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
        using MemoryStream memoryStream = new();
        await using StreamWriter writer = new(memoryStream, Encoding.UTF8, leaveOpen: true);

        Document.WriteTextTo(writer);
        await writer.FlushAsync();

        memoryStream.Position = 0;

        await File.SaveAsync(memoryStream);
    }

    private void OnDocumentTextChanged(object? sender, EventArgs e)
    {
        IsSaved = false;
    }

    [RelayCommand]
    private void CloseTab()
    {
        notifications.Debug("Close Editor Tab");

        RequestClose();
    }
}