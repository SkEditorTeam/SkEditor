using System;
using System.IO;
using System.IO.Abstractions;
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
    IFileSystem fileSystem,
    ILogService logs,
    IProjectService projects,
    Lazy<ISidebarService> sidebar)
    : ISessionRestoreService
{
    public static readonly string SessionFilePath = Path.Combine(AppPaths.LocalData, "session.bin");

    public async Task SaveSessionState()
    {
        if (!appSettings.IsSessionRestoreEnabled)
        {
            if (fileSystem.File.Exists(SessionFilePath))
            {
                fileSystem.File.Delete(SessionFilePath);
            }

            return;
        }

        SessionStateFile[] stateFiles = tabs.AllTabs
            .OfType<ISaveableTab>()
            .Where(t => t is IFileTab { File: LocalFile })
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

        await using FileSystemStream fileStream = fileSystem.File.Create(SessionFilePath);

        SessionState state = new(stateFiles)
        {
            SelectedTabHeader = tabs.SelectedTab?.Header?.Value,
            OpenFolderPath = projects.CurrentProjectPath,
            OpenedSidebarPanel = sidebar.Value.SelectedPanelName
        };

        await MemoryPackSerializer.SerializeAsync(fileStream, state);
    }

    public void RecoverSessionState()
    {
        bool fileExists = fileSystem.File.Exists(SessionFilePath);

        if (!fileExists || !appSettings.IsSessionRestoreEnabled)
        {
            return;
        }

        // ReSharper disable once MethodHasAsyncOverload
        byte[] bytes = fileSystem.File.ReadAllBytes(SessionFilePath);
        SessionState? state;
        try
        {
            state = MemoryPackSerializer.Deserialize<SessionState>(bytes);
        }
        catch (Exception e)
        {
            fileSystem.File.Delete(SessionFilePath);
            logs.Error(e, "Restoring session failed");
            return;
        }

        if (state is null)
        {
            return;
        }

        foreach (SessionStateFile file in state.Files)
        {
            bool isTarget = (file.Header ?? Path.GetFileName(file.Path)) == state.SelectedTabHeader;

            FileTabViewModel? tab;
            if (file.Content is null && file.Path is not null && file.IsSaved)
            {
                if (!fileSystem.File.Exists(file.Path))
                {
                    continue;
                }
                
                tab = fileTabFactory.CreateFromFile(new LocalFile(file.Path));
                tabs.OpenTab(tab, isTarget);
                _ = RestoreFileTabAsync(tab, file);
            }
            else
            {
                tab = fileTabFactory.CreateFromSession(file);
                tab.Document.Text = file.Content ?? string.Empty;
                tab.Document.UndoStack.ClearAll();
                tabs.OpenTab(tab, isTarget);

                RestoreTabState(tab, file);
            }
        }

        tabs.SelectedTab ??= tabs.AllTabs.LastOrDefault();

        if (state.OpenFolderPath is not null && fileSystem.Directory.Exists(state.OpenFolderPath))
        {
            projects.OpenProject(state.OpenFolderPath);
        }

        if (state.OpenedSidebarPanel is not null)
        {
            sidebar.Value.OpenPanel(state.OpenedSidebarPanel);
        }
    }

    private async Task RestoreFileTabAsync(FileTabViewModel tab, SessionStateFile sessionFile)
    {
        bool success = await files.LoadAndReadFileAsync(tab);
        if (success)
        {
            RestoreTabState(tab, sessionFile);
        }
    }

    private static void RestoreTabState(FileTabViewModel tab, SessionStateFile sessionFile)
    {
        tab.CaretOffset = sessionFile.CaretOffset;
        tab.ScrollOffset = sessionFile.ScrollOffset;
    }
}