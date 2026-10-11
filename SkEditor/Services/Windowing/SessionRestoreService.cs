using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using MemoryPack;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Windowing;
using SkEditor.Models.Files;
using SkEditor.Models.Session;
using SkEditor.Utilities;
using SkEditor.ViewModels.Tabs;
using SkEditor.ViewModels.Tabs.Capabilities;

namespace SkEditor.Services.Windowing;

public class SessionRestoreService(IFileTabFactory fileTabFactory, IFileService files, ITabService tabs)
    : ISessionRestoreService
{
    public async Task SaveSessionState()
    {
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
        string path = Path.Combine(AppPaths.LocalData, "session.bin");
        if (!File.Exists(path))
        {
            return;
        }

        // ReSharper disable once MethodHasAsyncOverload
        byte[] bytes = File.ReadAllBytes(path);
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
            else if (file.Content is not null)
            {
                tab = fileTabFactory.CreateFromSession(file);
                tab.Document.Text = file.Content;
                tab.Document.UndoStack.ClearAll();
                tabs.OpenTab(tab);
            }
            else
            {
                throw new InvalidOperationException("The session state file is malformed!");
            }

            tab.CaretOffset = file.CaretOffset;
            tab.ScrollOffset = file.ScrollOffset;
        }
    }
}