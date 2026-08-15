using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;
using SkEditor.Models.Analyzer;

namespace SkEditor.Views.Controls.StatusBar;

public partial class DrawerErrorEntry : UserControl
{
    public static readonly StyledProperty<string> ErrorMessageProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, string>(nameof(ErrorMessage));

    public static readonly StyledProperty<AnalyzerErrorType> ErrorTypeProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, AnalyzerErrorType>(nameof(ErrorType));

    public static readonly StyledProperty<int> ErrorLineProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, int>(nameof(ErrorLine));

    public static readonly StyledProperty<string> ErrorSourceProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, string>(nameof(ErrorSource));

    public static readonly DirectProperty<DrawerErrorEntry, Symbol> ErrorIconProperty =
        AvaloniaProperty.RegisterDirect<DrawerErrorEntry, Symbol>(
            nameof(ErrorIcon),
            o => o.ErrorIcon);

    public static readonly DirectProperty<DrawerErrorEntry, ImmutableSolidColorBrush> ErrorColorProperty =
        AvaloniaProperty.RegisterDirect<DrawerErrorEntry, ImmutableSolidColorBrush>(
            nameof(ErrorColor),
            o => o.ErrorColor);

    public static readonly StyledProperty<IRelayCommand> ErrorClickCommandProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, IRelayCommand>(nameof(ErrorClickCommand));

    public static readonly StyledProperty<object> ErrorClickCommandParameterProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, object>(nameof(ErrorClickCommandParameter));

    public IRelayCommand ErrorClickCommand
    {
        get => GetValue(ErrorClickCommandProperty);
        set => SetValue(ErrorClickCommandProperty, value);
    }

    public object ErrorClickCommandParameter
    {
        get => GetValue(ErrorClickCommandParameterProperty);
        set => SetValue(ErrorClickCommandParameterProperty, value);
    }

    public ImmutableSolidColorBrush ErrorColor
    {
        get;
        private set => SetAndRaise(ErrorColorProperty, ref field, value);
    } = new(Color.Parse("#c4c4c4"));

    public Symbol ErrorIcon
    {
        get;
        private set => SetAndRaise(ErrorIconProperty, ref field, value);
    } = Symbol.QuestionCircle;

    public int ErrorLine
    {
        get => GetValue(ErrorLineProperty);
        set => SetValue(ErrorLineProperty, value);
    }

    public string ErrorMessage
    {
        get => GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public string ErrorSource
    {
        get => GetValue(ErrorSourceProperty);
        set => SetValue(ErrorSourceProperty, value);
    }

    public AnalyzerErrorType ErrorType
    {
        get => GetValue(ErrorTypeProperty);
        set => SetValue(ErrorTypeProperty, value);
    }

    public DrawerErrorEntry()
    {
        InitializeComponent();
        UpdateIconAndColor();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ErrorTypeProperty)
        {
            UpdateIconAndColor();
        }
    }

    private void UpdateIconAndColor()
    {
        ErrorIcon = ErrorType switch
        {
            AnalyzerErrorType.Warning => Symbol.Warning,
            AnalyzerErrorType.Error => Symbol.ErrorCircle,
            _ => Symbol.QuestionCircle
        };

        ErrorColor = ErrorType switch
        {
            AnalyzerErrorType.Error => new ImmutableSolidColorBrush(Color.Parse("#FF3559")),
            AnalyzerErrorType.Warning => new ImmutableSolidColorBrush(Color.Parse("#FFB800")),
            _ => new ImmutableSolidColorBrush(Color.Parse("#c4c4c4"))
        };
    }
}