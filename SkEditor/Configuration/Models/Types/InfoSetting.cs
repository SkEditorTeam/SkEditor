using R3;
using SkEditor.Core.Localization;

namespace SkEditor.Configuration.Models.Types;

public class InfoSetting(BindableReactiveProperty<TranslatableString> name) : SettingElement<InfoSetting>(name)
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
        return new InfoSetting(new BindableReactiveProperty<TranslatableString>(name));
    }

    public static InfoSetting Create(BindableReactiveProperty<TranslatableString> name)
    {
        return new InfoSetting(name);
    }

    public InfoSetting WithType(InfoType type)
    {
        Type = type;
        return this;
    }
}