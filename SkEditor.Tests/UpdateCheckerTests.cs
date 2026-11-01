using System.IO.Abstractions.TestingHelpers;
using AwesomeAssertions;
using NSubstitute;
using RichardSzalay.MockHttp;
using Semver;
using SkEditor.Configuration.Configs;
using SkEditor.Core.Common;
using SkEditor.Core.Dialogs;
using SkEditor.Core.Extensions;
using SkEditor.Core.Logging;
using SkEditor.Core.Notifications;
using SkEditor.Core.System;
using SkEditor.Core.Updates;
using SkEditor.Core.Updates.Models;
using SkEditor.Tests.Common;
using SkEditor.Windowing;

namespace SkEditor.Tests;

public class UpdateCheckerTests
{
    private const string ReleasesJson = """
                                        [
                                            {"tag_name":"v3.0.0-alpha.24","prerelease":true},
                                            {"tag_name":"v2.9.3","prerelease":false},
                                            {"tag_name":"v2.9.2","prerelease":false}
                                        ]
                                        """;

    [Theory]
    [InlineData("2.9.2", false, true)]
    [InlineData("2.9.3", false, false)]
    [InlineData("2.9.3", true, true)]
    [InlineData("3.0.0-alpha.24", true, false)]
    [InlineData("3.0.0-alpha.23", true, true)]
    public async Task CheckForUpdates_ShouldReturnExpectedResult(
        string currentVersion,
        bool includePreReleases,
        bool shouldUpdate)
    {
        MockHttpMessageHandler mockHttp = new();
        mockHttp.When("https://api.github.com/repos/SkEditorTeam/SkEditor/releases")
            .Respond("application/json", ReleasesJson);
        
        MockFileSystem mockFileSystem = new();

        UpdateService service = new(
            mockHttp.ToHttpClientFactory(),
            new AppSettings { IncludePreReleaseUpdates = includePreReleases },
            new AppInfo(SemVersion.Parse(currentVersion, SemVersionStyles.Any)),
            Substitute.For<ILogService>(),
            Substitute.For<INotificationService>(),
            Substitute.For<IDialogService>(),
            mockFileSystem,
            Substitute.For<ILinkService>(),
            Substitute.For<IWindowRegistry>()
        );

        ReleaseInfo? result = await service.CheckForUpdates();
        if (shouldUpdate)
        {
            result.Should().NotBeNull();
            result.Version.IsNewerThan(SemVersion.Parse(currentVersion, SemVersionStyles.Any)).Should().BeTrue();
        }
        else
        {
            result.Should().BeNull();
        }
    }
}