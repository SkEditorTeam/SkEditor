using System.Threading.Tasks;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Downloading.Components;

public interface IAnalyzerComponent
{
    TranslatableString Name { get; }

    Task<DownloadInfo?> GetDownloadInfoAsync();
}