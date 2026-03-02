using System.Collections.Generic;
using SkEditor.Models.Menu;

namespace SkEditor.Contracts.Services;

public interface IMenuService
{
    IEnumerable<MainMenuItem> GetMainMenuItems();
}