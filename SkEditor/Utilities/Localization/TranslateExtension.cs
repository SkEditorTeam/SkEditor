using System;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;

namespace SkEditor.Utilities.Localization;

public class TranslateExtension : MarkupExtension
{
    public TranslateExtension() { }
    public TranslateExtension(TranslationKey key)
    {
        Key = key;
    }

    public TranslationKey? Key { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var dynamicResource = new DynamicResourceExtension(Key.ToString() ?? throw new InvalidOperationException("TranslationKey cannot be null."));
        return dynamicResource.ProvideValue(serviceProvider);
    }
}