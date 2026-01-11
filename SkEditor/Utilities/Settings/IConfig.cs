using System.Threading.Tasks;

namespace SkEditor.Utilities.Settings;

public interface IConfig
{
    string FileName { get; }
    Task Load();
    Task Save();
}