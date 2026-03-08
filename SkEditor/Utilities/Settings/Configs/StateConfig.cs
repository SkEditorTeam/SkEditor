using System.Collections.Generic;
using System.Text.Json.Serialization.Metadata;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings.Configs;

[SettingsConfig]
public partial class StateConfig : BaseConfig<StateConfig>
{
    protected override JsonTypeInfo<StateConfig> TypeInfo => AppJsonContext.Default.StateConfig;

    /// <summary>
    ///     The last used service for publishing scripts.
    /// </summary>
    public string LastUsedPublishService
    {
        get;
        set => SetProperty(ref field, value);
    } = "Pastebin";

    /// <summary>
    ///     The sizes of the sidebar panels.
    /// </summary>
    public Dictionary<string, int> SidebarPanelSizes
    {
        get;
        set => SetProperty(ref field, value);
    } = new();

    /// <summary>
    ///     The file type choices made by the user when opening files.
    /// </summary>
    // TODO: reconsider this entire system (i'm a bit lost, wasn't that the same as PreferredFileAssociations?) and whether this option should belong in state config
    public Dictionary<string, string?> FileTypeChoices
    {
        get;
        set => SetProperty(ref field, value);
    } = new();

    /// <summary>
    ///     Whether to use the skript-gui addon syntax in the GUI Generator instead of the default Skript syntax.
    /// </summary>
    public bool GuiGeneratorSkriptGuiAddonUsed
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    public double MainWindowWidth
    {
        get;
        set => SetProperty(ref field, value);
    } = 800;

    public double MainWindowHeight
    {
        get;
        set => SetProperty(ref field, value);
    } = 600;

    public int MainWindowX
    {
        get;
        set => SetProperty(ref field, value);
    } = 100;

    public int MainWindowY
    {
        get;
        set => SetProperty(ref field, value);
    } = 100;

    public bool MainWindowMaximized
    {
        get;
        set => SetProperty(ref field, value);
    } = false;

    protected override string GetFileName()
    {
        return "state.json";
    }
}