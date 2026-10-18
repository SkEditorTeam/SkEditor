using AwesomeAssertions;
using SkEditor.Workspace.Projects;

namespace SkEditor.Tests;

public class SftpConnectionStringParserTests
{
    [Theory]
    [InlineData("sftp://admin:secret@192.168.1.1:2222/home/server", "192.168.1.1", 2222, "admin", "secret",
        "/home/server")]
    [InlineData("sftp://user@example.com", "example.com", 22, "user", null, "/")]
    public void TryParse_ValidUrl_ParsesCorrectly(string input, string expectedHost, int expectedPort,
        string expectedUser, string? expectedPass, string expectedRoot)
    {
        bool success = SftpConnectionStringParser.TryParse(input, out SftpConnectionInfo? result);

        success.Should().BeTrue();
        result!.Host.Should().Be(expectedHost);
        result.Port.Should().Be(expectedPort);
        result.Username.Should().Be(expectedUser);
        result.Password.Should().Be(expectedPass);
        result.RootPath.Should().Be(expectedRoot);
    }
}