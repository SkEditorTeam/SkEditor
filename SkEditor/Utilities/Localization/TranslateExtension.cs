using System;
using System.ComponentModel;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace SkEditor.Utilities.Localization;

public class TranslateExtension : MarkupExtension
{
    public TranslateExtension() { }
    public TranslateExtension(TrKey key)
    {
        Key = key;
    }
    
    public TrKey Key { get; set; } = TrKey.Empty;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var dynamicResource = new DynamicResourceExtension(Key.ToString());
        return dynamicResource.ProvideValue(serviceProvider);
    }
}

public enum TrKey
{
    Empty,
    WelcomePageRecentTitle
}