using System.Threading.Tasks;

namespace SkEditor.Analyzing.Downloading;

public interface IAnalyzerDownloadHelper
{
    bool CheckComponents();
    Task Download();
}