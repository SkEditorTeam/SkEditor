using R3;
using SkEditor.Core.Localization;
using TranslatableString = SkEditor.Core.Localization.TranslatableString;

namespace SkEditor.Configuration.Models.Types;

public interface IFontChoiceSetting : IChoiceSetting;

public class FontChoiceSetting(BindableReactiveProperty<TranslatableString> name)
    : ChoiceSetting<string?>(name), IFontChoiceSetting
{
    public new static FontChoiceSetting Create(TranslatableString name)
    {
        return new FontChoiceSetting(new BindableReactiveProperty<TranslatableString>(name));
    }

    public new static FontChoiceSetting Create(BindableReactiveProperty<TranslatableString> name)
    {
        return new FontChoiceSetting(name);
    }
}