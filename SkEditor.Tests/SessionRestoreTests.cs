using System.IO.Abstractions.TestingHelpers;
using Avalonia;
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
using SkEditor.ViewModels.Tabs.Capabilities;

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
        ILogService logs = Substitute.For<ILogService>();

        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);
        fileSystem.AddFile(SessionRestoreService.SessionFilePath, new MockFileData(Guid.NewGuid().ToString()));

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem, logs);

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
        ILogService logs = Substitute.For<ILogService>();

        tabs.SelectedTab.Returns((ITab?)null);
        
        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem, logs);

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
        ILogService logs = Substitute.For<ILogService>();

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

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem, logs);

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
        ILogService logs = Substitute.For<ILogService>();

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

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem, logs);

        await sessionRestore.RecoverSessionState();

        tabs.Received(1).OpenTab(dummyTab, Arg.Any<bool>());
        dummyTab.Document.Text.Should().Be("Test");
        dummyTab.CaretOffset.Should().Be(1);
    }

    [Fact]
    public async Task RecoverSessionState_WhenFileDoesNotExist_DoNothing()
    {
        AppSettings appSettings = new() { IsSessionRestoreEnabled = true };

        IFileTabFactory fileTabFactory = Substitute.For<IFileTabFactory>();
        IFileService files = Substitute.For<IFileService>();
        ITabService tabs = Substitute.For<ITabService>();
        ILogService logs = Substitute.For<ILogService>();

        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem, logs);

        await sessionRestore.RecoverSessionState();

        fileTabFactory.ReceivedCalls().Should().BeEmpty();
        tabs.ReceivedCalls().Should().BeEmpty();
        files.ReceivedCalls().Should().BeEmpty();
    }

    [Fact]
    public async Task RecoverSessionState_SavedFile_LoadsFromFileAndOpens()
    {
        AppSettings appSettings = new() { IsSessionRestoreEnabled = true };

        IFileTabFactory fileTabFactory = Substitute.For<IFileTabFactory>();
        IFileService files = Substitute.For<IFileService>();
        ITabService tabs = Substitute.For<ITabService>();
        ILogService logs = Substitute.For<ILogService>();

        FileTabViewModel dummyTab = CreateDummyTab("saved.sk");
        fileTabFactory.CreateFromFile(Arg.Any<BaseFile>()).Returns(dummyTab);

        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);

        SessionStateFile mockStateFile = new()
        {
            IsSaved = true,
            Path = @"C:\projects\saved.sk",
            Content = null,
            Header = "saved.sk",
            CaretOffset = 42,
            ScrollOffset = new Vector(10, 20)
        };
        SessionState mockState = new([mockStateFile]);
        byte[] bytes = MemoryPackSerializer.Serialize(mockState);

        fileSystem.AddFile(SessionRestoreService.SessionFilePath, new MockFileData(bytes));

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem, logs);

        await sessionRestore.RecoverSessionState();

        fileTabFactory.Received(1).CreateFromFile(Arg.Is<BaseFile>(f => f.Path == @"C:\projects\saved.sk"));
        await files.Received(1).LoadAndOpenFileTabAsync(dummyTab, Arg.Any<bool>());
        tabs.DidNotReceiveWithAnyArgs().OpenTab(null!);

        dummyTab.CaretOffset.Should().Be(42);
        dummyTab.ScrollOffset.Should().Be(new Vector(10, 20));
    }

    [Fact]
    public async Task SaveSessionState_WithTabs_MapsSavedAndUnsavedCorrectly()
    {
        AppSettings appSettings = new() { IsSessionRestoreEnabled = true };

        IFileTabFactory fileTabFactory = Substitute.For<IFileTabFactory>();
        IFileService files = Substitute.For<IFileService>();
        ITabService tabs = Substitute.For<ITabService>();
        ILogService logs = Substitute.For<ILogService>();

        FileTabViewModel savedTab = CreateDummyTab("saved.sk", new LocalFile(@"C:\projects\saved.sk", "saved.sk"));
        savedTab.IsSaved = true;
        savedTab.Document.Text = "Saved";
        savedTab.CaretOffset = 15;
        savedTab.ScrollOffset = new Vector(0, 50);

        FileTabViewModel unsavedTab = CreateDummyTab("unsaved.sk");
        unsavedTab.IsSaved = false;
        unsavedTab.Document.Text = "Unsaved";
        unsavedTab.CaretOffset = 5;

        tabs.AllTabs.Returns([savedTab, unsavedTab]);
        tabs.SelectedTab.Returns(savedTab);

        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);

        SessionRestoreService sessionRestore = new(appSettings, fileTabFactory, files, tabs, fileSystem, logs);

        await sessionRestore.SaveSessionState();

        fileSystem.FileExists(SessionRestoreService.SessionFilePath).Should().BeTrue();
        byte[] bytes = fileSystem.GetFile(SessionRestoreService.SessionFilePath).Contents;
        SessionState? savedState = MemoryPackSerializer.Deserialize<SessionState>(bytes);

        savedState.Should().NotBeNull();
        savedState.Files.Should().HaveCount(2);
        savedState.SelectedTabHeader.Should().Be("saved.sk");

        SessionStateFile savedFile = savedState.Files[0];
        savedFile.IsSaved.Should().BeTrue();
        savedFile.Path.Should().Be(@"C:\projects\saved.sk");
        savedFile.Content.Should().BeNull();
        savedFile.CaretOffset.Should().Be(15);
        savedFile.ScrollOffset.Should().Be(new Vector(0, 50));

        SessionStateFile unsavedFile = savedState.Files[1];
        unsavedFile.IsSaved.Should().BeFalse();
        unsavedFile.Path.Should().BeNull();
        unsavedFile.Content.Should().Be("Unsaved");
        unsavedFile.CaretOffset.Should().Be(5);
    }

    private static FileTabViewModel CreateDummyTab(string header = "Test", LocalFile? file = null)
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
            File = file ?? new LocalFile(name: header)
        };
    }
}