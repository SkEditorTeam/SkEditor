using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using NSubstitute;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Dialogs.Models;
using SkEditor.Core.Localization;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.System;
using SkEditor.Editor.Features;
using SkEditor.Editor.Features.Saving;
using SkEditor.Editor.Tabs.Capabilities;
using SkEditor.Editor.UI.Tabs;
using SkEditor.Workspace.Files;
using SkEditor.Workspace.Files.Types;

namespace SkEditor.Tests;

public class ChangeDetectFeatureTests
{
    private const string TestFilePath = @"C:\projects\script.sk";

    [Fact]
    public void CanAttach_WhenTextEditorTabWithLocalFile_ReturnsTrue()
    {
        ChangeDetectFeature sut = CreateSut();
        FileTabViewModel tab = CreateDummyTab(TestFilePath, "content");

        bool result = sut.CanAttach(tab);
        result.Should().BeTrue();
    }

    [Fact]
    public void CanAttach_WhenTabIsNotTextEditorOrLocalFile_ReturnsFalse()
    {
        ChangeDetectFeature sut = CreateSut();
        IFeatureTab nonFileTab = Substitute.For<IFeatureTab>();

        bool result = sut.CanAttach(nonFileTab);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task CheckChanges_WhenFileDoesNotExistOnDisk_DoesNothing()
    {
        IDialogService dialogs = Substitute.For<IDialogService>();
        MockFileSystem fileSystem = new();

        ChangeDetectFeature sut = CreateSut(dialogs, fileSystem);
        FileTabViewModel tab = CreateDummyTab(TestFilePath, "content", fileSystem: fileSystem);

        await sut.CheckChangesAsync(tab);

        await dialogs.DidNotReceive().ShowDialogAsync(Arg.Any<Dialog>());
        tab.Document.Text.Should().Be("content");
    }

    [Fact]
    public async Task CheckChanges_WhenTimestampMatchesLastSaveUtc_DoesNothing()
    {
        IDialogService dialogs = Substitute.For<IDialogService>();
        MockFileSystem fileSystem = new();
        DateTime now = DateTime.UtcNow;

        fileSystem.AddFile(TestFilePath, new MockFileData("disk content") { LastWriteTime = now });

        ChangeDetectFeature sut = CreateSut(dialogs, fileSystem);
        FileTabViewModel tab = CreateDummyTab(TestFilePath, "editor content", fileSystem: fileSystem);
        tab.File.LastSaveUtc = now;

        await sut.CheckChangesAsync(tab);

        await dialogs.DidNotReceive().ShowDialogAsync(Arg.Any<Dialog>());
        tab.Document.Text.Should().Be("editor content");
    }

    [Fact]
    public async Task CheckChanges_WhenLengthDiffers_AndTabIsSaved_AutomaticallyReloadsContent()
    {
        IDialogService dialogs = Substitute.For<IDialogService>();
        MockFileSystem fileSystem = new();

        fileSystem.AddFile(TestFilePath, new MockFileData("new updated long content on disk")
        {
            LastWriteTime = DateTime.UtcNow.AddMinutes(1)
        });

        ChangeDetectFeature sut = CreateSut(dialogs, fileSystem);
        FileTabViewModel tab = CreateDummyTab(TestFilePath, "old", true, fileSystem);
        tab.File.Length = 3;
        tab.File.LastSaveUtc = DateTime.UtcNow;

        await sut.CheckChangesAsync(tab);

        await dialogs.DidNotReceive().ShowDialogAsync(Arg.Any<Dialog>());
        tab.Document.Text.Should().Be("new updated long content on disk");
    }

    [Fact]
    public async Task CheckChanges_WhenLengthDiffers_AndTabIsUnsaved_AndUserAccepts_ReloadsContent()
    {
        IDialogService dialogs = Substitute.For<IDialogService>();
        dialogs.ShowDialogAsync(Arg.Any<Dialog>()).Returns(DialogResult.Ok);

        MockFileSystem fileSystem = new();
        fileSystem.AddFile(TestFilePath, new MockFileData("modified disk content")
        {
            LastWriteTime = DateTime.UtcNow.AddMinutes(1)
        });

        ChangeDetectFeature sut = CreateSut(dialogs, fileSystem);
        FileTabViewModel tab = CreateDummyTab(TestFilePath, "unsaved user changes", false, fileSystem);
        tab.File.Length = 5;
        tab.File.LastSaveUtc = DateTime.UtcNow;

        await sut.CheckChangesAsync(tab);

        await dialogs.Received(1).ShowDialogAsync(Arg.Any<Dialog>());
        tab.Document.Text.Should().Be("modified disk content");
    }

    [Fact]
    public async Task CheckChanges_WhenLengthDiffers_AndTabIsUnsaved_AndUserCancels_KeepsContentAndUpdatesSnapshot()
    {
        IDialogService dialogs = Substitute.For<IDialogService>();
        dialogs.ShowDialogAsync(Arg.Any<Dialog>()).Returns(DialogResult.Cancel);

        MockFileSystem fileSystem = new();
        DateTime diskTime = DateTime.UtcNow.AddMinutes(5);
        fileSystem.AddFile(TestFilePath, new MockFileData("disk content")
        {
            LastWriteTime = diskTime
        });

        ChangeDetectFeature sut = CreateSut(dialogs, fileSystem);
        FileTabViewModel tab = CreateDummyTab(TestFilePath, "my custom edits", false, fileSystem);
        tab.File.Length = 5;
        tab.File.LastSaveUtc = DateTime.UtcNow;

        await sut.CheckChangesAsync(tab);

        await dialogs.Received(1).ShowDialogAsync(Arg.Any<Dialog>());
        tab.Document.Text.Should().Be("my custom edits");
        tab.File.LastSaveUtc.Should().Be(diskTime);
    }

    [Fact]
    public async Task CheckChanges_WhenLengthMatches_ButHashDiffers_ReloadsContent()
    {
        IDialogService dialogs = Substitute.For<IDialogService>();
        MockFileSystem fileSystem = new();

        fileSystem.AddFile(TestFilePath, new MockFileData("test")
        {
            LastWriteTime = DateTime.UtcNow.AddMinutes(1)
        });

        ChangeDetectFeature sut = CreateSut(dialogs, fileSystem);
        FileTabViewModel tab = CreateDummyTab(TestFilePath, "demo", true, fileSystem);
        tab.File.Length = 4;
        tab.File.DiskHash = 123456789;
        tab.File.LastSaveUtc = DateTime.UtcNow;

        await sut.CheckChangesAsync(tab);

        tab.Document.Text.Should().Be("test");
        tab.File.DiskHash.Should().NotBe(123456789);
    }

    [Fact]
    public async Task CheckChanges_WhenHashMatchesButTimestampDiffers_UpdatesSnapshot()
    {
        MockFileSystem fs = new();
        IDialogService dialogs = Substitute.For<IDialogService>();

        DateTime oldTime = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        DateTime newTime = oldTime.AddMinutes(1);

        fs.AddFile(TestFilePath, new MockFileData("same")
        {
            LastWriteTime = newTime
        });

        FileTabViewModel tab = CreateDummyTab(
            TestFilePath, "same", fileSystem: fs);

        using MemoryStream ms = new([.. "same"u8]);
        await tab.File.UpdateSnapshotAsync(ms);
        tab.File.LastSaveUtc = oldTime;

        ChangeDetectFeature sut = CreateSut(dialogs, fs);

        await sut.CheckChangesAsync(tab);

        tab.File.LastSaveUtc.Should().Be(newTime);
        tab.Document.Text.Should().Be("same");

        await dialogs.DidNotReceive()
            .ShowDialogAsync(Arg.Any<Dialog>());
    }

    private static FileTabViewModel CreateDummyTab(
        string path,
        string content,
        bool isSaved = true,
        IFileSystem? fileSystem = null)
    {
        IFileSystem fs = fileSystem ?? new MockFileSystem();
        LocalFile localFile = new(path, Path.GetFileName(path), fs);

        FileTabViewModel tab = new(
            TranslatableString.FromRawString(Path.GetFileName(path)),
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
            File = localFile,
            Document =
            {
                Text = content
            },
            IsSaved = isSaved
        };

        return tab;
    }

    private static ChangeDetectFeature CreateSut(
        IDialogService? dialogs = null,
        IFileSystem? fileSystem = null)
    {
        return new ChangeDetectFeature(
            new AppSettings { IsChangeDetectEnabled = true },
            dialogs ?? Substitute.For<IDialogService>(),
            fileSystem ?? new MockFileSystem()
        );
    }
}