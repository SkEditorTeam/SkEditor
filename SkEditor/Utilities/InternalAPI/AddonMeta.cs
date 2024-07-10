using SkEditor.API;
using System.Collections.Generic;
using System.Linq;

namespace SkEditor.Utilities.InternalAPI;

public class AddonMeta
{

    public IAddon Addon { get; set; } = null!;
    public IAddons.AddonState State { get; set; } = IAddons.AddonState.Installed;
    public List<IAddonLoadingError> Errors { get; set; }
    public string? DllFilePath { get; set; } = null;
    public bool NeedsRestart { get; set; } = false;
    public AddonLoadContext LoadContext { get; set; } = null!;

    public bool HasErrors => Errors.Count > 0;
    public bool HasCriticalErrors => Errors.Any(x => x.IsCritical);
}