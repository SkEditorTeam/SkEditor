using Semver;

namespace SkEditor.Core.Updates.Models;

public record ReleaseInfo(GitHubRelease Release, SemVersion Version);