using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using CommunityToolkit.Mvvm.Input;
using FluentIcons.Common;

namespace SkEditor.Views.Controls.StatusBar;

public partial class DrawerErrorEntry : UserControl
{
    public static readonly StyledProperty<string> ErrorMessageProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, string>(nameof(ErrorMessage));

    public string ErrorMessage
    {
        get => GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public enum ErrorType
    {
        Info,
        Warning,
        Error
    }

    public static readonly StyledProperty<ErrorType> ErrorTypeProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, ErrorType>(nameof(ErrorType));

    public ErrorType Type
    {
        get => GetValue(ErrorTypeProperty);
        set => SetValue(ErrorTypeProperty, value);
    }

    public static readonly StyledProperty<int> ErrorLineProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, int>(nameof(ErrorLine));

    public int ErrorLine
    {
        get => GetValue(ErrorLineProperty);
        set => SetValue(ErrorLineProperty, value);
    }

    public static readonly StyledProperty<string> ErrorSourceProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, string>(nameof(ErrorSource));

    public string ErrorSource
    {
        get => GetValue(ErrorSourceProperty);
        set => SetValue(ErrorSourceProperty, value);
    }

    public static readonly DirectProperty<DrawerErrorEntry, Symbol> ErrorIconProperty =
        AvaloniaProperty.RegisterDirect<DrawerErrorEntry, Symbol>(
            nameof(ErrorIcon),
            o => o.ErrorIcon);

    public Symbol ErrorIcon
    {
        get;
        private set => SetAndRaise(ErrorIconProperty, ref field, value);
    } = Symbol.Info;

    public static readonly DirectProperty<DrawerErrorEntry, ImmutableSolidColorBrush> ErrorColorProperty =
        AvaloniaProperty.RegisterDirect<DrawerErrorEntry, ImmutableSolidColorBrush>(
            nameof(ErrorColor),
            o => o.ErrorColor);

    public ImmutableSolidColorBrush ErrorColor
    {
        get;
        private set => SetAndRaise(ErrorColorProperty, ref field, value);
    } = new(Color.Parse("#4D9AFF"));

    public static readonly StyledProperty<IRelayCommand> ErrorClickCommandProperty =
        AvaloniaProperty.Register<DrawerErrorEntry, IRelayCommand>(nameof(ErrorClickCommand));

    public IRelayCommand ErrorClickCommand
    {
        get => GetValue(ErrorClickCommandProperty);
        set => SetValue(ErrorClickCommandProperty, value);
    }
    
    public DrawerErrorEntry() => InitializeComponent();

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
        ErrorIcon = Type switch
        {
            ErrorType.Info => Symbol.Info,
            ErrorType.Warning => Symbol.Warning,
            ErrorType.Error => Symbol.ErrorCircle,
            _ => Symbol.Info
        };

        ErrorColor = Type switch
        {
            ErrorType.Error => new ImmutableSolidColorBrush(Color.Parse("#FF3559")),
            ErrorType.Warning => new ImmutableSolidColorBrush(Color.Parse("#FFB800")),
            ErrorType.Info => new ImmutableSolidColorBrush(Color.Parse("#4D9AFF")),
            _ => new ImmutableSolidColorBrush(Color.Parse("#FF4D4D"))
        };
    }
}