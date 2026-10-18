using System.Collections.Generic;
using SkEditor.Layout.Menu.Models;
using SkEditor.Layout.Menu.Providers;

namespace SkEditor.Layout.Menu;

public class MenuService(IEnumerable<IMenuProvider> menuProviders) : IMenuService
{
    public IEnumerable<MainMenuItem> GetMainMenuItems()
    {
        foreach (IMenuProvider menuProvider in menuProviders)
        {
            yield return menuProvider.GetMainMenuItem();
        }
    }
}