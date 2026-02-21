using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Shapes;

namespace AquaSync.App.Controls;

public sealed partial class ScheduleEditorControl : UserControl
{
    // ── Dependency Properties ────────────────────────────────────────────────

    public static readonly DependencyProperty SunriseTimeProperty =
        DependencyProperty.Register(
            nameof(SunriseTime),
            typeof(object),
            typeof(ScheduleEditorControl),
            new PropertyMetadata(null, (d, _) => ((ScheduleEditorControl)d).RedrawTimeline()));

    public static readonly DependencyProperty SunsetTimeProperty =
        DependencyProperty.Register(
            nameof(SunsetTime),
            typeof(object),
            typeof(ScheduleEditorControl),
            new PropertyMetadata(null, (d, _) => ((ScheduleEditorControl)d).RedrawTimeline()));

    public static readonly DependencyProperty RampUpMinutesProperty =
        DependencyProperty.Register(
            nameof(RampUpMinutes),
            typeof(int),
            typeof(ScheduleEditorControl),
            new PropertyMetadata(0, (d, _) => ((ScheduleEditorControl)d).RedrawTimeline()));

    public TimeOnly? SunriseTime
    {
        get => GetValue(SunriseTimeProperty) is TimeOnly t ? t : null;
        set => SetValue(SunriseTimeProperty, (object?)value);
    }

    public TimeOnly? SunsetTime
    {
        get => GetValue(SunsetTimeProperty) is TimeOnly t ? t : null;
        set => SetValue(SunsetTimeProperty, (object?)value);
    }

    public int RampUpMinutes
    {
        get => (int)GetValue(RampUpMinutesProperty);
        set => SetValue(RampUpMinutesProperty, value);
    }

    // ── State ────────────────────────────────────────────────────────────────

    private Ellipse? _draggingHandle;

    public ScheduleEditorControl()
    {
        InitializeComponent();
    }

    // ── Pointer Handling ─────────────────────────────────────────────────────

    private void Handle_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        _draggingHandle = (Ellipse)sender;
        _draggingHandle.CapturePointer(e.Pointer);
        e.Handled = true;
    }

    private void Handle_PointerMoved(object sender, PointerRoutedEventArgs e)
    {
        if (_draggingHandle is null || TimelineCanvas.ActualWidth == 0)
            return;

        var x = e.GetCurrentPoint(TimelineCanvas).Position.X;
        var fraction = Math.Clamp(x / TimelineCanvas.ActualWidth, 0.0, 1.0);
        var totalMinutes = (int)(fraction * 1440);
        var time = new TimeOnly(totalMinutes / 60, totalMinutes % 60);

        if (_draggingHandle == SunriseHandle)
            SunriseTime = time;
        else
            SunsetTime = time;

        e.Handled = true;
    }

    private void Handle_PointerReleased(object sender, PointerRoutedEventArgs e)
    {
        if (_draggingHandle is null)
            return;

        _draggingHandle.ReleasePointerCapture(e.Pointer);
        _draggingHandle = null;
        e.Handled = true;
    }

    // ── Drawing ──────────────────────────────────────────────────────────────

    private void OnSizeChanged(object sender, SizeChangedEventArgs e) => RedrawTimeline();

    private void RedrawTimeline()
    {
        var width = TimelineCanvas.ActualWidth;
        var height = TimelineCanvas.ActualHeight;

        if (width == 0)
            return;

        if (SunriseTime is null || SunsetTime is null)
        {
            // Blank state: OffLeft fills full width, everything else hidden.
            Canvas.SetLeft(OffLeft, 0);
            OffLeft.Width = width;
            OffLeft.Height = height;

            RampSegment.Width = 0;
            OnSegment.Width = 0;
            OffRight.Width = 0;

            SunriseHandle.Visibility = Visibility.Collapsed;
            SunsetHandle.Visibility = Visibility.Collapsed;

            SunriseLabel.Text = string.Empty;
            SunsetLabel.Text = string.Empty;
            return;
        }

        var sunriseMinutes = SunriseTime.Value.Hour * 60 + SunriseTime.Value.Minute;
        var sunsetMinutes = SunsetTime.Value.Hour * 60 + SunsetTime.Value.Minute;
        var rampEndMinutes = Math.Min(sunriseMinutes + RampUpMinutes, sunsetMinutes);

        var sunriseX = sunriseMinutes / 1440.0 * width;
        var rampEndX = rampEndMinutes / 1440.0 * width;
        var sunsetX = sunsetMinutes / 1440.0 * width;

        // OffLeft: 0 → sunriseX
        Canvas.SetLeft(OffLeft, 0);
        OffLeft.Width = sunriseX;
        OffLeft.Height = height;

        // RampSegment: sunriseX → rampEndX (hidden when no ramp)
        Canvas.SetLeft(RampSegment, sunriseX);
        RampSegment.Width = RampUpMinutes > 0 ? Math.Max(0, rampEndX - sunriseX) : 0;
        RampSegment.Height = height;

        // OnSegment: rampEndX → sunsetX
        Canvas.SetLeft(OnSegment, rampEndX);
        OnSegment.Width = Math.Max(0, sunsetX - rampEndX);
        OnSegment.Height = height;

        // OffRight: sunsetX → end
        Canvas.SetLeft(OffRight, sunsetX);
        OffRight.Width = Math.Max(0, width - sunsetX);
        OffRight.Height = height;

        // Sunrise handle
        const double handleSize = 16;
        Canvas.SetLeft(SunriseHandle, sunriseX - handleSize / 2);
        Canvas.SetTop(SunriseHandle, height / 2 - handleSize / 2);
        SunriseHandle.Visibility = Visibility.Visible;

        // Sunset handle
        Canvas.SetLeft(SunsetHandle, sunsetX - handleSize / 2);
        Canvas.SetTop(SunsetHandle, height / 2 - handleSize / 2);
        SunsetHandle.Visibility = Visibility.Visible;

        SunriseLabel.Text = SunriseTime.Value.ToString("HH:mm");
        SunsetLabel.Text = SunsetTime.Value.ToString("HH:mm");
    }
}
