using System.Threading.Tasks;
using R3;
using SkEditor.Core.Localization;

namespace SkEditor.Analyzing.Downloading;

public interface IAnalyzerDownloadHelper
{
    BindableReactiveProperty<bool> IsDownloading { get; }
    BindableReactiveProperty<float> Progress { get; }
    BindableReactiveProperty<TranslatableString> ProgressDescription { get; }
    
    bool CheckComponents();
    Task Download();
}