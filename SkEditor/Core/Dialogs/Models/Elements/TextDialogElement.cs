using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using SkEditor.Core.Dialogs.Common;
using SkEditor.Core.Extensions;
using SkEditor.Core.Localization;

namespace SkEditor.Core.Dialogs.Models.Elements;

[ObservableObject]
public partial class TextDialogElement : DialogElement
{
    public required BindableReactiveProperty<TranslatableString> Text { get; set; }
    public double FontSize { get; set; } = 14;
    public ElementAlignment Alignment { get; set; } = ElementAlignment.Left;
    public ElementMargin Margin { get; set; } = ElementMargin.Uniform(0);
    
    public bool IsBold { get; set; }
    public bool IsItalic { get; set; }
    
    public static TextDialogElement Create(string text)
    {
        return new TextDialogElement
        {
            Text = new BindableReactiveProperty<TranslatableString>(text.ToTranslatable())
        };
    }
    
    public static TextDialogElement Create(TranslatableString text)
    {
        return new TextDialogElement
        {
            Text = new BindableReactiveProperty<TranslatableString>(text)
        };
    }
}