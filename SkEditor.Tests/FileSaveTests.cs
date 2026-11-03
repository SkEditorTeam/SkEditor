using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using NSubstitute;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Extensions;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Editor.Features;
using SkEditor.Editor.Features.Saving;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs;
using SkEditor.Workspace.Tabs.Templates;

namespace SkEditor.Tests;

public class FileSaveTests
{
    [Fact]
    public void FileTab_ShouldBeMarkedAsUnsaved_EvenIfAutoSaveIsEnabled_WhenDocumentIsModified()
    {
        AppSettings appSettings = new() { IsAutoSaveEnabled = true };
        ExperimentsConfig experimentsConfig = new();
        MockFileSystem mockFileSystem = new();
        mockFileSystem.AddFile("/path/to/file.txt", new MockFileData("Test"));

        FileTabViewModel fileTabViewModel = new("Test".ToTranslatable(), appSettings, experimentsConfig)
        {
            File = new LocalFile("/path/to/file.txt", fileSystem: mockFileSystem)
        };

        FileService fileService = CreateFileService();

        new SaveIndicatorFeature().Attach(fileTabViewModel);
        new AutoSaveFeature(appSettings, fileService).Attach(fileTabViewModel);
        
        fileTabViewModel.Document.Text = "newcontent";
        
        fileTabViewModel.IsSaved.Should().BeFalse();
    }

    [Fact]
    public async Task CloseTabAsync_ShouldSaveFileToDisk_WhenAutoSaveIsEnabledAndDocumentIsModifiedQuickly()
    {
        AppSettings appSettings = new() { IsAutoSaveEnabled = true };
        ExperimentsConfig experimentsConfig = new();
        MockFileSystem mockFileSystem = new();
        mockFileSystem.AddFile("/path/to/file.txt", new MockFileData("Test"));

        FileTabViewModel fileTabViewModel = new("Test".ToTranslatable(), appSettings, experimentsConfig)
        {
            File = new LocalFile("/path/to/file.txt", fileSystem: mockFileSystem)
        };

        FileService fileService = CreateFileService();

        new SaveIndicatorFeature().Attach(fileTabViewModel);
        new AutoSaveFeature(appSettings, fileService).Attach(fileTabViewModel);

        fileTabViewModel.Document.Text = "newcontent";

        IDialogService dialogService = Substitute.For<IDialogService>();
        dialogService.ShowDialogAsync(Arg.Any<Dialog>()).Returns(DialogResult.Cancel);

        TabService tabService = new(
            Substitute.For<IFileFeatureService>(),
            new TabViewLocator(),
            fileService,
            dialogService,
            Substitute.For<INotificationService>()
        );
        
        await tabService.CloseTabAsync(fileTabViewModel);
        
        mockFileSystem.FileExists("/path/to/file.txt").Should().BeTrue();
        mockFileSystem.GetFile("/path/to/file.txt").TextContents.Should().Be("newcontent");
    }

    private static FileService CreateFileService() => new(
        Substitute.For<IFilePickerService>(),
        Substitute.For<IRecentFileService>(),
        Substitute.For<INotificationService>(),
        Substitute.For<IDialogService>(),
        Substitute.For<ILogService>()
    );
}