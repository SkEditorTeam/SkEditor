using R3;
using SkEditor.Core.Localization;

namespace SkEditor.Configuration.Models.Types;

public class ProgressSetting(BindableReactiveProperty<TranslatableString> name) : SettingElement<ProgressSetting>(name)
{
    public BindableReactiveProperty<TranslatableString> Text { get; private set; } = new();
    public BindableReactiveProperty<float> Value { get; private set; } = new();
    public BindableReactiveProperty<bool> IsIndeterminate { get; private set; } = new();
    public override double? Width { get; set; }

    public static ProgressSetting Create(TranslatableString name)
    {
        return new ProgressSetting(new BindableReactiveProperty<TranslatableString>(name));
    }

    public static ProgressSetting Create(BindableReactiveProperty<TranslatableString> name)
    {
        return new ProgressSetting(name);
    }

    public ProgressSetting WithValue(BindableReactiveProperty<float> value)
    {
        Value = value;
        return this;
    }
    
    public ProgressSetting WithText(BindableReactiveProperty<TranslatableString> text)
    {
        Text = text;
        return this;
    }
    
    public ProgressSetting WithWidth(double width)
    {
        Width = width;
        return this;
    }
    
    public ProgressSetting WithIndeterminate(bool isIndeterminate)
    {
        IsIndeterminate = new BindableReactiveProperty<bool>(isIndeterminate);
        return this;
    }
    
    public ProgressSetting WithIndeterminate(BindableReactiveProperty<bool> isIndeterminate)
    {
        IsIndeterminate = isIndeterminate;
        return this;
    }
}