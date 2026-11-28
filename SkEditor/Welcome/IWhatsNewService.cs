using System.Collections.Generic;
using SkEditor.Welcome.Models;

namespace SkEditor.Welcome;

public interface IWhatsNewService
{
    void ShowWhatsNew();
    
    IReadOnlyList<WhatsNewFeaturedItem> GetFeaturedItems();
}