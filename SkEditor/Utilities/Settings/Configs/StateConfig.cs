using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;

namespace SkEditor.Utilities.Settings.Configs;

public class StateConfig : BaseConfig<StateConfig>
{
    protected override JsonTypeInfo<StateConfig> TypeInfo => AppJsonContext.Default.StateConfig;
    
    public override string FileName => "state.json";
    
    /// <summary>
    /// The last used service for publishing scripts.
    /// </summary>
    public string LastUsedPublishService
    {
        get;
        set => SetProperty(ref field, value);
    } = "Pastebin";
    
    /// <summary>
    /// The sizes of the sidebar panels.
    /// </summary>
    public Dictionary<string, int> SidebarPanelSizes
    {
        get;
        set => SetProperty(ref field, value);
    } = new();
    
    /// <summary>
    /// The file type choices made by the user when opening files.
    /// </summary>
    // TODO: reconsider this entire system and whether this option should belong in state config
    public Dictionary<string, string?> FileTypeChoices
    {
        get;
        set => SetProperty(ref field, value);
    } = new();
    
    /// <summary>
    /// Whether to use the skript-gui addon syntax in the GUI Generator instead of the default Skript syntax.
    /// </summary>
    public bool GuiGeneratorSkriptGuiAddonUsed
    {
        get;
        set => SetProperty(ref field, value);
    } = false;
    
    protected override void CopyFrom(StateConfig other)
    {
        LastUsedPublishService = other.LastUsedPublishService;
        SidebarPanelSizes = new Dictionary<string, int>(other.SidebarPanelSizes);
        FileTypeChoices = new Dictionary<string, string?>(other.FileTypeChoices);
        GuiGeneratorSkriptGuiAddonUsed = other.GuiGeneratorSkriptGuiAddonUsed;
    }
}