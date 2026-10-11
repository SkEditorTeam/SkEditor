using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using MemoryPack;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Files;
using SkEditor.Models.Session;
using SkEditor.Utilities;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Services.Windowing;

public class SessionRestoreService(
    AppSettings appSettings,
    IFileTabFactory fileTabFactory,
    IFileService files,
    ITabService tabs,
    ILogService logs)
    : ISessionRestoreService
{
    private static readonly string SessionFilePath = Path.Combine(AppPaths.LocalData, "session.bin");

    public async Task SaveSessionState()
    {
        if (!appSettings.IsSessionRestoreEnabled)
        {
            if (File.Exists(SessionFilePath))
            {
                File.Delete(SessionFilePath);
            }

            return;
        }

        SessionStateFile[] stateFiles = tabs.AllTabs
            .OfType<ISaveableTab>()
            .Select(tab =>
            {
                bool isSaved = tab is { IsSaved: true };
                string? content = null;
                int caretOffset = 0;
                Vector scrollOffset = default;
                if (tab is ITextEditorTab editorTab)
                {
                    caretOffset = editorTab.CaretOffset;
                    scrollOffset = editorTab.ScrollOffset;

                    if (!isSaved)
                    {
                        content = editorTab.Document.Text;
                    }
                }

                string? header = tab switch
                {
                    ITab headerTab => headerTab.Header.Value,
                    _ => null
                };

                return new SessionStateFile
                {
                    IsSaved = isSaved,
                    CaretOffset = caretOffset,
                    ScrollOffset = scrollOffset,
                    Path = tab is IFileTab fileTab ? fileTab.Path : null,
                    Header = header,
                    Content = content
                };
            })
            .ToArray();

        FileStream fileStream = File.Create(Path.Combine(AppPaths.LocalData, "session.bin"));
        SessionState state = new(stateFiles);
        await MemoryPackSerializer.SerializeAsync(fileStream, state);
    }

    public async Task RecoverSessionState()
    {
        bool fileExists = File.Exists(SessionFilePath);

        if (!fileExists || !appSettings.IsSessionRestoreEnabled)
        { 
            return;
        }

        // ReSharper disable once MethodHasAsyncOverload
        byte[] bytes = File.ReadAllBytes(SessionFilePath);
        SessionState? state = MemoryPackSerializer.Deserialize<SessionState>(bytes);
        if (state is null)
        {
            return;
        }

        foreach (SessionStateFile file in state.Files)
        {
            FileTabViewModel? tab;
            if (file.Content is null && file.Path is not null && file.IsSaved)
            {
                tab = fileTabFactory.CreateFromFile(new LocalFile(file.Path));
                await files.LoadAndOpenFileTabAsync(tab);
            }
            else
            {
                tab = fileTabFactory.CreateFromSession(file);
                tab.Document.Text = file.Content ?? string.Empty;
                tab.Document.UndoStack.ClearAll();
                tabs.OpenTab(tab);
            }

            tab.CaretOffset = file.CaretOffset;
            tab.ScrollOffset = file.ScrollOffset;
        }
    }
}