using System.IO;
using System.Text;
using System.Threading.Tasks;
using AvaloniaEdit.Document;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SkEditor.Contracts.Services;
using SkEditor.Models.Files;
using SkEditor.Utilities.Localization;

namespace SkEditor.Models.Tabs;

public partial class FileTab(TranslatableString header) : Tab(header)
{
    public required BaseFile File { get; init; }
    public TextDocument Document { get; } = new();

    public override IRelayCommand CloseCommand => CloseTabCommand;

    [RelayCommand]
    private void CloseTab()
    {
        App.Services.GetRequiredService<INotificationService>().Debug("Close Editor Tab");

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

    public static FileTab CreateFromFile(string path)
    {
        string fileName = Path.GetFileName(path);
        TranslatableString header = TranslatableString.FromRawString(fileName);

        return new FileTab(header)
        {
            File = new LocalFile(fileName, path)
        };
    }

    public static FileTab CreateNewFile(string name)
    {
        TranslatableString header = TranslatableString.FromRawString(name);

        return new FileTab(header)
        {
            File = new LocalFile(name)
        };
    }
}