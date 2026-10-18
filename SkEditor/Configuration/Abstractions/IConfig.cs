using System.ComponentModel;
using System.Threading.Tasks;

namespace SkEditor.Utilities.Settings;

public interface IConfig : INotifyPropertyChanged
{
    string FileName { get; }
    Task Load();
    Task Save();
    Task SaveImmediate();
}