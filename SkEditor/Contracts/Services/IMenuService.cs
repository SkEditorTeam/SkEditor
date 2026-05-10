using System.Collections.Generic;
using SkEditor.ViewModels.Menu;

namespace SkEditor.Contracts.Services;

public interface IMenuService
{
    IEnumerable<MainMenuItem> GetMainMenuItems();
}