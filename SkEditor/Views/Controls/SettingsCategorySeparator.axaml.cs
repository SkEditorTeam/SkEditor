using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using SkEditor.Services.Localization;
using SkEditor.Utilities.Localization;

namespace SkEditor.Views.Controls;

public partial class SettingsCategorySeparator : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<SettingsCategorySeparator, string?>(nameof(Title));

    public static readonly StyledProperty<TranslationKey?> TitleKeyProperty =
        AvaloniaProperty.Register<SettingsCategorySeparator, TranslationKey?>(nameof(TitleKey));

    public SettingsCategorySeparator() => InitializeComponent();

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public TranslationKey? TitleKey
    {
        get => GetValue(TitleKeyProperty);
        set => SetValue(TitleKeyProperty, value);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        UpdateTitle();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        // SkEditorAPI.Events.OnLanguageChanged += OnLanguageChanged;
        UpdateTitle();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        // SkEditorAPI.Events.OnLanguageChanged -= OnLanguageChanged;
    }

    // private void OnLanguageChanged(object? sender, LanguageChangedEventArgs e)
    // {
    //     UpdateTitle();
    // }

    private void UpdateTitle()
    {
        string? label = "";
        if (TitleKey != null)
        {
            label = TranslationService.Current.Get(TitleKey.Value);
        }
        else if (Title != null)
        {
            label = Title;
        }

        Dispatcher.UIThread.Post(() => { TitleBlock.Text = label; });
    }
}