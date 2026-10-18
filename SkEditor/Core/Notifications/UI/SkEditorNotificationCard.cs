using System;
using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Threading;

namespace SkEditor.Core.Notifications.UI;

public class SkEditorNotificationCard : NotificationCard
{
    private TimeSpan _expiration;
    private DispatcherTimer? _timer;

    protected override Type StyleKeyOverride => typeof(NotificationCard);

    public void StartTimer(TimeSpan expiration)
    {
        _expiration = expiration;
        if (expiration == TimeSpan.Zero)
        {
            return;
        }

        _timer = new DispatcherTimer { Interval = expiration };
        _timer.Tick += (_, _) =>
        {
            _timer.Stop();
            Close();
        };
        _timer.Start();
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        _timer?.Stop();
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        if (_expiration != TimeSpan.Zero)
        {
            _timer?.Start();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer?.Stop();
    }
}