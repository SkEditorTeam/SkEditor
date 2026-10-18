using System;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using SkEditor.Core.Localization;

namespace SkEditor.Core.Localization;

public class TranslateExtension : MarkupExtension
{
    public TranslationKey? Key { get; set; }

    public TranslateExtension()
    {
    }

    public TranslateExtension(TranslationKey key) => Key = key;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        DynamicResourceExtension dynamicResource =
            new(Key.ToString() ?? throw new InvalidOperationException("TranslationKey cannot be null."));
        return dynamicResource.ProvideValue(serviceProvider);
    }
}