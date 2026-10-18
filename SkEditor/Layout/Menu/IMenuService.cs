using System.Collections.Generic;
using SkEditor.Layout.Menu.Models;

namespace SkEditor.Layout.Menu;

public interface IMenuService
{
    IEnumerable<MainMenuItem> GetMainMenuItems();
}