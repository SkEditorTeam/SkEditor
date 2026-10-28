using CommunityToolkit.Mvvm.ComponentModel;
using R3;
using SkEditor.Core.Localization;

namespace SkEditor.Core.Dialogs.Models.Elements;

[ObservableObject]
public partial class TextDialogElement : DialogElement
{
    public required BindableReactiveProperty<TranslatableString> Text { get; set; }
}