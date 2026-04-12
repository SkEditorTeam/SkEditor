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
    : Tab(header)
{
    public required BaseFile File { get; init; }
    public TextDocument Document { get; } = new();

    public AppSettings AppSettings { get; } = appSettings;

    public override IRelayCommand CloseCommand => CloseTabCommand;

    [RelayCommand]
    private void CloseTab()
    {
        notifications.Debug("Close Editor Tab");

        RequestClose();
    }

    public async Task LoadContentAsync()
    {
        if (string.IsNullOrEmpty(File.Path))
        {
            return;
        }

        await using var stream = await File.LoadAsync();
        using var reader = new StreamReader(stream);

        Document.Text = await reader.ReadToEndAsync();
        Document.UndoStack.ClearAll();
    }

    public async Task SaveContentAsync()
    {
        using var memoryStream = new MemoryStream();
        await using var writer = new StreamWriter(memoryStream, Encoding.UTF8, leaveOpen: true);

        Document.WriteTextTo(writer);
        await writer.FlushAsync();

        memoryStream.Position = 0;

        await File.SaveAsync(memoryStream);
    }
}