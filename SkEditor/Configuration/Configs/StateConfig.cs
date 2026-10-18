using System;
using System.Text.Json.Serialization.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using SkEditor.Utilities.Settings.Attributes;

namespace SkEditor.Utilities.Settings.Configs;

[SettingsConfig]
public partial class StateConfig : BaseConfig<StateConfig>
{
    /// <summary>
    ///     The height of the bottom drawer pane.
    /// </summary>
    [ObservableProperty]
    public partial double BottomDrawerHeight { get; set; } = 170;

    /// <summary>
    ///     Whether to use the skript-gui addon syntax in the GUI Generator instead of the default Skript syntax.
    /// </summary>
    [ObservableProperty]
    public partial bool GuiGeneratorSkriptGuiAddonUsed { get; set; } = false;

    /// <summary>
    ///     The last used service for publishing scripts.
    /// </summary>
    [ObservableProperty]
    public partial string LastUsedPublishService { get; set; } = "Pastebin";

    [ObservableProperty]
    public partial double MainWindowHeight { get; set; } = 1000;

    [ObservableProperty]
    public partial bool MainWindowMaximized { get; set; } = true;

    [ObservableProperty]
    public partial double MainWindowWidth { get; set; } = 1800;

    [ObservableProperty]
    public partial int MainWindowX { get; set; } = 100;

    [ObservableProperty]
    public partial int MainWindowY { get; set; } = 100;

    /// <summary>
    ///     The size of the sidebar pane.
    /// </summary>
    [ObservableProperty]
    public partial double SidebarPaneSize { get; set; } = 320;


    protected override Environment.SpecialFolder TargetFolder => Environment.SpecialFolder.LocalApplicationData;
    protected override JsonTypeInfo<StateConfig> TypeInfo => AppJsonContext.Default.StateConfig;

    protected override string GetFileName()
    {
        return "state.json";
    }
}