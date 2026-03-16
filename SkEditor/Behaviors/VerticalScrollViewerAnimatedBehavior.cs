using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using SkEditor.Utilities.Extensions;

namespace SkEditor.Behaviors;

public class VerticalScrollViewerAnimatedBehavior : StyledElementBehavior<ScrollViewer>
{
    public enum ChangeSize
    {
        Line,
        Page
    }

    private const double AnimationDuration = 170;

    public static readonly DirectProperty<VerticalScrollViewerAnimatedBehavior, double> ScrollStepSizeProperty =
        AvaloniaProperty.RegisterDirect<VerticalScrollViewerAnimatedBehavior, double>(nameof(ScrollStepSize),
            o => o.ScrollStepSize,
            (o, v) => o.ScrollStepSize = v);

    public static readonly StyledProperty<ChangeSize> ScrollChangeSizeProperty =
        AvaloniaProperty.Register<VerticalScrollViewerAnimatedBehavior, ChangeSize>(nameof(ScrollChangeSize));

    private DateTime _animationStartTime;

    private bool _isAnimating;

    private ScrollContentPresenter? _scp;
    private double _startOffset;
    private double _targetOffset;

    public double ScrollStepSize
    {
        get;
        set => SetAndRaise(ScrollStepSizeProperty, ref field, value);
    } = 100;

    public ChangeSize ScrollChangeSize
    {
        get => GetValue(ScrollChangeSizeProperty);
        set => SetValue(ScrollChangeSizeProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject!.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged,
            RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject!.SetValue(ScrollChangeSizeProperty, ChangeSize.Line);

        AssociatedObject.Loaded += AssociatedObject_Loaded;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject!.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
    }

    private void AssociatedObject_Loaded(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        _scp = AssociatedObject?.Presenter as ScrollContentPresenter;

        AssociatedObject!.Loaded -= AssociatedObject_Loaded;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!IsEnabled)
        {
            e.Handled = !_scp?.IsScrollChainingEnabled ?? false;
            return;
        }

        _scp ??= AssociatedObject?.Presenter as ScrollContentPresenter;

        if (_scp == null)
        {
            e.Handled = !_scp?.IsScrollChainingEnabled ?? false;
            return;
        }

        object? src = e.Source;
        while (src != null && !Equals(src, _scp))
        {
            if (src is ScrollContentPresenter scp2)
            {
                if (scp2 == _scp)
                {
                    break;
                }

                if ((e.Delta.Y > 0 && scp2.Offset.Y == 0) ||
                    (e.Delta.Y < 0 &&
                     Math.Abs(scp2.Offset.Y - (scp2.Extent.Height - scp2.Viewport.Height)) < 0.1))
                {
                    src = scp2.GetVisualParent();
                }
                else
                {
                    return;
                }
            }
            else if (src is Visual visual)
            {
                src = visual.GetVisualParent();
            }
        }

        if (!Equals(src, _scp))
        {
            e.Handled = !(src as ScrollContentPresenter)?.IsScrollChainingEnabled ?? false;
            return;
        }

        Vector delta = e.Delta;
        double x = _scp!.Offset.X;
        double y = _scp!.Offset.Y;
        double maxOffsetY = _scp!.Extent.Height - _scp!.Viewport.Height;

        ILogicalScrollable? scrollable = _scp?.Child as ILogicalScrollable;
        bool isLogical = scrollable?.IsLogicalScrollEnabled == true;
        if (_scp!.Extent.Height > _scp!.Viewport.Height)
        {
            double height = isLogical ? scrollable!.ScrollSize.Height : ScrollStepSize;
            y += -delta.Y * height;
            y = Math.Max(y, 0);
            y = Math.Min(y, maxOffsetY);
        }

        Vector newOffset = SnapOffset(new Vector(x, y), delta, true);
        double step = Math.Abs(newOffset.Y - _scp!.Offset.Y);

        if (delta.Y > 0)
        {
            if (ScrollChangeSize == ChangeSize.Line)
            {
                AnimateScroll(-step);
            }
            else
            {
                AnimateScroll(-AssociatedObject!.Bounds.Height);
            }
        }
        else
        {
            AnimateScroll(ScrollChangeSize == ChangeSize.Line ? step : AssociatedObject!.Bounds.Height);
        }

        bool offsetChanged = newOffset != _scp.Offset;
        e.Handled = !_scp.IsScrollChainingEnabled || offsetChanged;
    }

    private void AnimateScroll(double delta)
    {
        DateTime currentTime = DateTime.Now;
        double maxOffset = AssociatedObject!.Extent.Height - AssociatedObject!.Bounds.Height;
        if (maxOffset < 0)
        {
            maxOffset = 0;
        }

        if (_isAnimating)
        {
            double elapsedTime = (currentTime - _animationStartTime).TotalMilliseconds;
            double progress = Math.Min(elapsedTime / AnimationDuration, 1.0);

            SineEaseOut easing = new();
            double easedProgress = easing.Ease(progress);

            _startOffset += easedProgress * (_targetOffset - _startOffset);

            _targetOffset += delta;
            _targetOffset = Math.Clamp(_targetOffset, 0, maxOffset);

            _animationStartTime = currentTime;
        }
        else
        {
            _isAnimating = true;
            _startOffset = AssociatedObject!.Offset.Y;
            _targetOffset = _startOffset + delta;

            _targetOffset = Math.Clamp(_targetOffset, 0, maxOffset);

            _animationStartTime = currentTime;
            Animate();
        }
    }

    private void Animate()
    {
        TopLevel? topLevel = TopLevel.GetTopLevel(AssociatedObject);
        if (topLevel == null)
        {
            _isAnimating = false;
            return;
        }

        Action<TimeSpan>? frameCallback = null;
        frameCallback = _ =>
        {
            if (!_isAnimating)
            {
                return;
            }

            double elapsedTime = (DateTime.Now - _animationStartTime).TotalMilliseconds;

            if (elapsedTime >= AnimationDuration)
            {
                AssociatedObject!.Offset = new Vector(AssociatedObject!.Offset.X, _targetOffset);
                _isAnimating = false;
                return;
            }

            double progress = elapsedTime / AnimationDuration;
            SineEaseOut easing = new();
            double easedProgress = easing.Ease(progress);

            double currentOffset = _startOffset + (easedProgress * (_targetOffset - _startOffset));

            AssociatedObject!.Offset = new Vector(AssociatedObject!.Offset.X, currentOffset);

            topLevel.RequestAnimationFrame(frameCallback!);
        };

        topLevel.RequestAnimationFrame(frameCallback);
    }

    #region ScrollContentPresenter

    private static (double previous, double next) FindNearestSnapPoint(IReadOnlyList<double> snapPoints, double value)
    {
        int point = snapPoints.BinarySearch(value, Comparer<double>.Default);

        double previousSnapPoint, nextSnapPoint;

        if (point < 0)
        {
            point = ~point;

            previousSnapPoint = snapPoints[Math.Max(0, point - 1)];
            nextSnapPoint = point >= snapPoints.Count ? snapPoints[^1] : snapPoints[Math.Max(0, point)];
        }
        else
        {
            previousSnapPoint = nextSnapPoint = snapPoints[Math.Max(0, point)];
        }

        return (previousSnapPoint, nextSnapPoint);
    }

    private IScrollSnapPointsInfo? GetScrollSnapPointsInfo(object? content)
    {
        object? scrollable = _scp!.Content switch
        {
            ItemsControl itemsControl => itemsControl.Presenter?.Panel,
            ItemsPresenter itemsPresenter => itemsPresenter.Panel,
            _ => content
        };

        IScrollSnapPointsInfo? snapPointsInfo = scrollable as IScrollSnapPointsInfo;

        return snapPointsInfo;
    }

    private Vector SnapOffset(Vector offset, Vector direction = default, bool snapToNext = false)
    {
        IScrollSnapPointsInfo? scrollable = GetScrollSnapPointsInfo(_scp!.Content);

        if (scrollable is null || _scp!.VerticalSnapPointsType == SnapPointsType.None)
        {
            return offset;
        }

        Vector diff = GetAlignmentDiff();

        IReadOnlyList<double>? verticalSnapPoints;
        double verticalSnapPoint = 0;
        double verticalSnapPointOffset = 0;

        bool areVerticalSnapPointsRegular = scrollable.AreVerticalSnapPointsRegular;

        if (!areVerticalSnapPointsRegular)
        {
            verticalSnapPoints =
                scrollable.GetIrregularSnapPoints(Orientation.Vertical, _scp!.VerticalSnapPointsAlignment);
        }
        else
        {
            verticalSnapPoints = new List<double>();
            verticalSnapPoint = scrollable.GetRegularSnapPoints(Orientation.Vertical, _scp!.VerticalSnapPointsAlignment,
                out verticalSnapPointOffset);
        }

        if (_scp!.VerticalSnapPointsType == SnapPointsType.None ||
            (!areVerticalSnapPointsRegular && !(verticalSnapPoints?.Count > 0)) ||
            (snapToNext && (!snapToNext || direction.Y == 0)))
        {
            return offset;
        }

        Vector estimatedOffset = new(offset.X, offset.Y + diff.Y);
        double previousSnapPoint = 0, nextSnapPoint = 0, midPoint = 0;

        if (areVerticalSnapPointsRegular)
        {
            previousSnapPoint = ((int)(estimatedOffset.Y / verticalSnapPoint) * verticalSnapPoint) +
                                verticalSnapPointOffset;
            nextSnapPoint = previousSnapPoint + verticalSnapPoint;
            midPoint = (previousSnapPoint + nextSnapPoint) / 2;
        }
        else if (verticalSnapPoints?.Count > 0)
        {
            (previousSnapPoint, nextSnapPoint) = FindNearestSnapPoint(verticalSnapPoints, estimatedOffset.Y);
            midPoint = (previousSnapPoint + nextSnapPoint) / 2;
        }

        double nearestSnapPoint = snapToNext
            ? direction.Y > 0 ? previousSnapPoint : nextSnapPoint
            : estimatedOffset.Y < midPoint
                ? previousSnapPoint
                : nextSnapPoint;

        offset = new Vector(offset.X, nearestSnapPoint - diff.Y);

        return offset;

        Vector GetAlignmentDiff()
        {
            Vector vector = default;

            switch (_scp!.VerticalSnapPointsAlignment)
            {
                case SnapPointsAlignment.Center:
                    vector += new Vector(0, _scp!.Viewport.Height / 2);
                    break;
                case SnapPointsAlignment.Far:
                    vector += new Vector(0, _scp!.Viewport.Height);
                    break;
            }

            switch (_scp!.HorizontalSnapPointsAlignment)
            {
                case SnapPointsAlignment.Center:
                    vector += new Vector(_scp!.Viewport.Width / 2, 0);
                    break;
                case SnapPointsAlignment.Far:
                    vector += new Vector(_scp!.Viewport.Width, 0);
                    break;
            }

            return vector;
        }
    }

    #endregion
}