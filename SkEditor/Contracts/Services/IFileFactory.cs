using System.Threading.Tasks;
using FluentAvalonia.UI.Controls;

namespace SkEditor.Contracts.Services;

public interface IFileFactory
{
    Task<TabViewItem?> Build(string header, string path = "", string? content = null);
}