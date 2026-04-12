using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings.Configs;

[SettingsConfig]
public partial class StateConfig : BaseConfig<StateConfig>
{
    protected override JsonTypeInfo<StateConfig> TypeInfo => AppJsonContext.Default.StateConfig;

    /// <summary>
    ///     The last used service for publishing scripts.
    /// </summary>
    [ObservableProperty]
    public partial string LastUsedPublishService { get; set; } = "Pastebin";

    /// <summary>
    ///     The sizes of the sidebar panels.
    /// </summary>
    [ObservableProperty]
    public partial Dictionary<string, int> SidebarPanelSizes { get; set; } = new();

    /// <summary>
    ///     The file type choices made by the user when opening files.
    /// </summary>
    // TODO: reconsider this entire system (i'm a bit lost, wasn't that the same as PreferredFileAssociations?) and whether this option should belong in state config
    [ObservableProperty]
    public partial Dictionary<string, string?> FileTypeChoices { get; set; } = new();

    /// <summary>
    ///     Whether to use the skript-gui addon syntax in the GUI Generator instead of the default Skript syntax.
    /// </summary>
    [ObservableProperty]
    public partial bool GuiGeneratorSkriptGuiAddonUsed { get; set; } = false;

    [ObservableProperty]
    public partial double MainWindowWidth { get; set; } = 1800;

    [ObservableProperty]
    public partial double MainWindowHeight { get; set; } = 1000;

    [ObservableProperty]
    public partial int MainWindowX { get; set; } = 100;

    [ObservableProperty]
    public partial int MainWindowY { get; set; } = 100;

    [ObservableProperty]
    public partial bool MainWindowMaximized { get; set; } = true;

    protected override string GetFileName()
    {
        return "state.json";
    }
}