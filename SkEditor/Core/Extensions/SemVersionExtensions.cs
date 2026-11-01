using Semver;

namespace SkEditor.Core.Extensions;

public static class SemVersionExtensions
{
    extension(SemVersion latest)
    {
        public bool IsNewerThan(SemVersion current) => latest.ComparePrecedenceTo(current) > 0;

        public bool IsOlderThan(SemVersion other) => latest.ComparePrecedenceTo(other) < 0;
    }
}