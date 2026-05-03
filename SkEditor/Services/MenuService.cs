using System.Collections.Generic;
using SkEditor.Contracts.Services;
using SkEditor.Models.Menu;
using SkEditor.Utilities.Menu;

namespace SkEditor.Services;

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