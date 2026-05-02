using SkEditor.Utilities.Localization;

namespace SkEditor.Utilities.Settings.Models.Types;

public class InfoSetting(TranslatableString name) : SettingElement<InfoSetting>(name)
{
    public enum InfoType
    {
        Info,
        Warning,
        Error
    }

    public InfoType Type { get; set; } = InfoType.Info;

    public static InfoSetting Create(TranslatableString name)
    {
        return new InfoSetting(name);
    }

    public InfoSetting WithType(InfoType type)
    {
        Type = type;
        return this;
    }
}