using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using MemoryPack;
using NSubstitute;
using SkEditor.Core.Common;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Windowing.Errors;
using SkEditor.Windowing.Errors.Models;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Types;
using SkEditor.Workspace.Tabs;
using AppSettings = SkEditor.Configuration.Configs.AppSettings;
using ExperimentsConfig = SkEditor.Configuration.Configs.ExperimentsConfig;

namespace SkEditor.Tests;

public class CrashServiceTests
{
    [Fact]
    public void SaveCrashState_WithVariousTabs_SerializesAndSavesCorrectly()
    {
        ITabService tabs = Substitute.For<ITabService>();
        MockFileSystem fileSystem = new();

        FileTabViewModel savedLocalTab = CreateDummyTab("saved.sk", new LocalFile(@"C:\projects\saved.sk", "saved.sk"));
        savedLocalTab.IsSaved = true;
        savedLocalTab.Document.Text = "Saved content";

        FileTabViewModel unsavedLocalTab =
            CreateDummyTab("unsaved.sk", new LocalFile(@"C:\projects\unsaved.sk", "unsaved.sk"));
        unsavedLocalTab.IsSaved = false;
        unsavedLocalTab.Document.Text = "Unsaved content";

        FileTabViewModel remoteTab =
            CreateDummyTab("remote.sk", new SftpFile(@"/home/user/remote.sk", "remote.sk", null!, null!));
        remoteTab.IsSaved = true;
        remoteTab.Document.Text = "Remote content";

        tabs.AllTabs.Returns([savedLocalTab, unsavedLocalTab, remoteTab]);

        CrashService crashService = CreateSut(tabs: tabs, fileSystem: fileSystem);

        crashService.SaveCrashState(new Exception("Dummy crash exception"));

        fileSystem.FileExists(CrashService.CrashRecoveryFilePath).Should().BeTrue();

        byte[] bytes = fileSystem.GetFile(CrashService.CrashRecoveryFilePath).Contents;
        CrashState? savedState = MemoryPackSerializer.Deserialize<CrashState>(bytes);

        savedState.Should().NotBeNull();
        savedState.Files.Should().HaveCount(3);

        savedState.Files[0].IsSaved.Should().BeTrue();
        savedState.Files[0].IsRemote.Should().BeFalse();
        savedState.Files[0].Content.Should().BeNull();

        savedState.Files[1].IsSaved.Should().BeFalse();
        savedState.Files[1].IsRemote.Should().BeFalse();
        savedState.Files[1].Content.Should().Be("Unsaved content");

        savedState.Files[2].IsSaved.Should().BeTrue();
        savedState.Files[2].IsRemote.Should().BeTrue();
        savedState.Files[2].Content.Should().Be("Remote content");
    }

    [Fact]
    public async Task CheckForCrashes_WhenNoRecoveryFile_DoesNothing()
    {
        IDialogService dialogs = Substitute.For<IDialogService>();
        MockFileSystem fileSystem = new();
        CrashService crashService = CreateSut(fileSystem, dialogs: dialogs);

        await crashService.CheckForCrashes();

        await dialogs.DidNotReceive().ShowDialogAsync(Arg.Any<Dialog>());
    }

    [Fact]
    public async Task CheckForCrashes_WhenDeserializationFails_DeletesFileAndExits()
    {
        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);
        fileSystem.AddFile(CrashService.CrashRecoveryFilePath, new MockFileData([0x01, 0x02, 0x03]));

        ILogService logs = Substitute.For<ILogService>();
        IDialogService dialogs = Substitute.For<IDialogService>();

        CrashService crashService = CreateSut(fileSystem, logs: logs, dialogs: dialogs);

        await crashService.CheckForCrashes();

        fileSystem.FileExists(CrashService.CrashRecoveryFilePath).Should().BeFalse();
        logs.Received().Error(Arg.Any<Exception>(), "Restoring pre-crash state failed");
        await dialogs.DidNotReceive().ShowDialogAsync(Arg.Any<Dialog>());
    }

    [Fact]
    public async Task CheckForCrashes_WhenUserClicksDiscard_DeletesFileAndIgnores()
    {
        MockFileSystem fileSystem = CreateFileSystemWithValidCrashState();
        ITabService tabs = Substitute.For<ITabService>();
        IDialogService dialogs = Substitute.For<IDialogService>();

        dialogs.ShowDialogAsync(Arg.Any<Dialog>()).Returns(DialogResult.No);

        CrashService crashService = CreateSut(fileSystem, tabs, dialogs: dialogs);

        await crashService.CheckForCrashes();

        await dialogs.Received(1).ShowDialogAsync(Arg.Any<Dialog>());
        fileSystem.FileExists(CrashService.CrashRecoveryFilePath).Should().BeFalse();
        tabs.DidNotReceive().OpenTab(Arg.Any<FileTabViewModel>());
    }

    [Fact]
    public async Task CheckForCrashes_WhenUserClicksRestore_RestoresTabsAndDeletesFile()
    {
        IFileTabFactory fileTabFactory = Substitute.For<IFileTabFactory>();
        IFileService files = Substitute.For<IFileService>();
        ITabService tabs = Substitute.For<ITabService>();
        IDialogService dialogs = Substitute.For<IDialogService>();

        dialogs.ShowDialogAsync(Arg.Any<Dialog>()).Returns(DialogResult.Yes);

        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);

        fileSystem.AddFile(@"C:\projects\saved.sk", new MockFileData("saved on disk"));
        CrashStateFile savedLocal = new() { IsSaved = true, Path = @"C:\projects\saved.sk", Header = "saved.sk" };

        CrashStateFile unsavedLocal = new()
            { IsSaved = false, Path = @"C:\projects\unsaved.sk", Header = "unsaved.sk", Content = "recovered text" };

        byte[] bytes = MemoryPackSerializer.Serialize(new CrashState([savedLocal, unsavedLocal]));
        fileSystem.AddFile(CrashService.CrashRecoveryFilePath, new MockFileData(bytes));

        FileTabViewModel dummySavedTab = CreateDummyTab("saved.sk");
        FileTabViewModel dummyUnsavedTab = CreateDummyTab("unsaved.sk");

        fileTabFactory.CreateFromFile(Arg.Is<BaseFile>(f => f.Path == @"C:\projects\saved.sk")).Returns(dummySavedTab);
        fileTabFactory.CreateFromCrash(Arg.Is<CrashStateFile>(f => f.Header == "unsaved.sk")).Returns(dummyUnsavedTab);

        CrashService crashService =
            CreateSut(fileSystem, tabs, fileTabFactory: fileTabFactory, files: files, dialogs: dialogs);

        await crashService.CheckForCrashes();

        fileTabFactory.Received(1).CreateFromFile(Arg.Any<BaseFile>());
        await files.Received(1).LoadAndReadFileAsync(dummySavedTab);

        fileTabFactory.ReceivedWithAnyArgs(1).CreateFromCrash(unsavedLocal);
        dummyUnsavedTab.Document.Text.Should().Be("recovered text");

        tabs.Received(1).OpenTab(dummySavedTab);
        tabs.Received(1).OpenTab(dummyUnsavedTab);

        fileSystem.FileExists(CrashService.CrashRecoveryFilePath).Should().BeFalse();
    }

    private static MockFileSystem CreateFileSystemWithValidCrashState()
    {
        MockFileSystem fileSystem = new();
        fileSystem.AddDirectory(AppPaths.LocalData);

        CrashStateFile dummyStateFile = new() { IsSaved = false, Header = "Test", Content = "Test content" };
        CrashState state = new([dummyStateFile]);

        byte[] bytes = MemoryPackSerializer.Serialize(state);
        fileSystem.AddFile(CrashService.CrashRecoveryFilePath, new MockFileData(bytes));

        return fileSystem;
    }

    private static FileTabViewModel CreateDummyTab(string header = "Test", BaseFile? file = null)
    {
        return new FileTabViewModel(
            TranslatableString.FromRawString(header),
            new AppSettings(),
            new ExperimentsConfig())
        {
            File = file ?? new LocalFile(name: header)
        };
    }

    private static CrashService CreateSut(
        IFileSystem? fileSystem = null,
        ITabService? tabs = null,
        ILogService? logs = null,
        IFileTabFactory? fileTabFactory = null,
        IFileService? files = null,
        IDialogService? dialogs = null)
    {
        return new CrashService(
            fileSystem ?? new MockFileSystem(),
            tabs ?? Substitute.For<ITabService>(),
            logs ?? Substitute.For<ILogService>(),
            fileTabFactory ?? Substitute.For<IFileTabFactory>(),
            files ?? Substitute.For<IFileService>(),
            dialogs ?? Substitute.For<IDialogService>()
        );
    }
}