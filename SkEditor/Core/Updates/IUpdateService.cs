using System.Threading.Tasks;
using SkEditor.Core.Updates.Models;

namespace SkEditor.Core.Updates;

public interface IUpdateService
{
    Task<ReleaseInfo?> CheckForUpdates();
    void ShowNotification(ReleaseInfo? info);
}