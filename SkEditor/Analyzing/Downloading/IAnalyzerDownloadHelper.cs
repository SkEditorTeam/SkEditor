using System.Threading.Tasks;
using R3;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Downloading;

public interface IAnalyzerDownloadHelper
{
    bool CheckComponents();
    Task Download(BindableReactiveProperty<float> progress, BindableReactiveProperty<TranslatableString> progressDescription);
}