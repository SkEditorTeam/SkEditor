using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using MemoryPack;
using NSubstitute;
using SkEditor.Contracts.Services;
using SkEditor.Contracts.Services.Files;
using SkEditor.Contracts.Services.Files.Features;
using SkEditor.Models.Files;
using SkEditor.Models.Session;
using SkEditor.Services.Windowing;
using SkEditor.Utilities;
using SkEditor.Utilities.Localization;
using SkEditor.Utilities.Settings;
using SkEditor.Utilities.Settings.Configs;
using SkEditor.ViewModels.Tabs;

namespace SkEditor.Tests;

public class SessionRestoreTests
{
    [Fact]
    public async Task SaveSessionState_WhenDisabled_FileDoesntExist()
    {
        AppSettings appSettings = new() { IsSessionRestoreEnabled = false };

        IFileTabFactory fileTabFactory = Substitute.For<IFileTabFactory>();
        IFileService files = Substitute.For<IFileService>();
        ITabService tabs = Substitute.For<ITabService>();

        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);
        fileSystem.AddFile(SessionRestoreService.SessionFilePath, new MockFileData(Guid.NewGuid().ToString()));

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem);

        await sessionRestore.SaveSessionState();

        fileSystem.FileExists(SessionRestoreService.SessionFilePath).Should().BeFalse();
    }

    [Fact]
    public async Task SaveSessionState_WhenEnabled_FileExists()
    {
        AppSettings appSettings = new() { IsSessionRestoreEnabled = true };

        IFileTabFactory fileTabFactory = Substitute.For<IFileTabFactory>();
        IFileService files = Substitute.For<IFileService>();
        ITabService tabs = Substitute.For<ITabService>();

        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem);

        await sessionRestore.SaveSessionState();

        fileSystem.FileExists(SessionRestoreService.SessionFilePath).Should().BeTrue();
        fileSystem.GetFile(SessionRestoreService.SessionFilePath).Contents.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RecoverSessionState_WhenDisabled_DoNothing()
    {
        AppSettings appSettings = new() { IsSessionRestoreEnabled = false };

        IFileTabFactory fileTabFactory = Substitute.For<IFileTabFactory>();
        IFileService files = Substitute.For<IFileService>();
        ITabService tabs = Substitute.For<ITabService>();

        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);

        SessionStateFile mockStateFile = new()
        {
            IsSaved = false,
            Content = "Test",
            Header = "New file 1",
            CaretOffset = 1,
            ScrollOffset = default
        };
        SessionState mockState = new([mockStateFile]);
        byte[] bytes = MemoryPackSerializer.Serialize(mockState);

        fileSystem.AddFile(SessionRestoreService.SessionFilePath, new MockFileData(bytes));

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem);

        await sessionRestore.RecoverSessionState();

        tabs.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task RecoverSessionState_SessionExists_Restore()
    {
        AppSettings appSettings = new() { IsSessionRestoreEnabled = true };

        IFileTabFactory fileTabFactory = Substitute.For<IFileTabFactory>();
        IFileService files = Substitute.For<IFileService>();
        ITabService tabs = Substitute.For<ITabService>();

        FileTabViewModel dummyTab = CreateDummyTab();
        fileTabFactory.CreateFromSession(Arg.Any<SessionStateFile>()).Returns(dummyTab);

        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);

        SessionStateFile mockStateFile = new()
        {
            IsSaved = false,
            Content = "Test",
            Header = "New file 1",
            CaretOffset = 1,
            ScrollOffset = default
        };
        SessionState mockState = new([mockStateFile]);
        byte[] bytes = MemoryPackSerializer.Serialize(mockState);

        fileSystem.AddFile(SessionRestoreService.SessionFilePath, new MockFileData(bytes));

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem);

        await sessionRestore.RecoverSessionState();

        tabs.Received(1).OpenTab(dummyTab);
        dummyTab.Document.Text.Should().Be("Test");
        dummyTab.CaretOffset.Should().Be(1);
    }

    private static FileTabViewModel CreateDummyTab(string header = "Test")
    {
        return new FileTabViewModel(
            TranslatableString.FromRawString(header),
            new AppSettings(),
            new ExperimentsConfig(),
            Substitute.For<IDialogService>(),
            Substitute.For<INotificationService>(),
            Substitute.For<IFilePickerService>(),
            Substitute.For<IFileFeatureService>(),
            Substitute.For<IRecentFileService>(),
            Substitute.For<ILinkService>(),
            Substitute.For<IClipboardService>(),
            Substitute.For<ILogService>())
        {
            File = new LocalFile(name: header)
        };
    }
}