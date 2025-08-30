using System;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.Foundation;                  // Point, Size
using MikoMe.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace MikoMe.Views;

public sealed partial class SessionPage : Page
{
    public SessionPage()
    {
        InitializeComponent();
        Loaded += SessionPage_Loaded;
        KeyDown += SessionPage_KeyDown;
        VM.OnVisibilityChanged += UpdateVisibility;
    }

    public MikoMe.ViewModels.SessionViewModel VM
        => (MikoMe.ViewModels.SessionViewModel)DataContext;

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        if (e.Parameter is CardDirection d)
            Tag = d;
    }

    private async void SessionPage_Loaded(object? sender, RoutedEventArgs e)
    {
        var dir = Tag is CardDirection d ? d : CardDirection.ZhToEn;
        await VM.InitAsync(dir);

        // Ensure Space works immediately
        _ = Focus(FocusState.Programmatic);

        UpdateVisibility();
        UpdateClock(); // initial arc
    }

    private void UpdateVisibility()
    {
        ShowButtonElement.Visibility = VM.IsAnswerShown ? Visibility.Collapsed : Visibility.Visible;
        AnswerTextElement.Visibility = VM.IsAnswerShown ? Visibility.Visible : Visibility.Collapsed;
        ActionsPanelElement.Visibility = VM.IsAnswerShown ? Visibility.Visible : Visibility.Collapsed;

        PinyinTextElement.Visibility = VM.ShowPinyin ? Visibility.Visible : Visibility.Collapsed;

        UpdateClock();
    }

    // Buttons
    private void ShowButton_Click(object sender, RoutedEventArgs e)
    {
        if (!VM.IsAnswerShown)
        {
            VM.IsAnswerShown = true;
            UpdateVisibility();
        }
    }

    private void Pass_Click(object sender, RoutedEventArgs e)
    {
        VM.PassCommand.Execute(null);
        UpdateVisibility();
    }

    private void Fail_Click(object sender, RoutedEventArgs e)
    {
        VM.FailCommand.Execute(null);
        UpdateVisibility();
    }

    private void Speak_Click(object sender, RoutedEventArgs e)
        => VM.SpeakCommand.Execute(null);

    // Keyboard
    private void SessionPage_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        switch (e.Key)
        {
            case Windows.System.VirtualKey.Space:
                if (!VM.IsAnswerShown) { VM.IsAnswerShown = true; }
                else { VM.PassCommand.Execute(null); }
                UpdateVisibility();
                e.Handled = true;
                break;

            case Windows.System.VirtualKey.K:
                VM.PassCommand.Execute(null); UpdateVisibility(); e.Handled = true; break;

            case Windows.System.VirtualKey.J:
                VM.FailCommand.Execute(null); UpdateVisibility(); e.Handled = true; break;

            case Windows.System.VirtualKey.S:
                VM.SpeakCommand.Execute(null); e.Handled = true; break;
        }
    }

    // Progress "clock" — parses "2/12"
    private void UpdateClock()
    {
        var parts = Regex.Matches(VM.ProgressText ?? string.Empty, @"\d+")
                         .Select(m => int.Parse(m.Value))
                         .ToArray();

        if (parts.Length >= 2 && parts[1] > 0)
        {
            int done = parts[0];
            int total = parts[1];
            double ratio = Math.Clamp(done / (double)total, 0.0, 1.0);

            RingText.Text = $"{done}/{total}";
            SetArc(ratio);
        }
        else
        {
            RingText.Text = VM.ProgressText ?? string.Empty;
            SetArc(0);
        }
    }

    private void SetArc(double ratio)
    {
        const double width = 120;
        const double height = 120;
        const double strokeInset = 3; // half of StrokeThickness (6/2)

        double r = (width / 2) - strokeInset;
        double cx = width / 2;
        double cy = height / 2;

        const double startAngle = -90; // 12 o'clock
        double sweep = 360 * Math.Clamp(ratio, 0, 1);
        double endAngle = startAngle + sweep;

        Point start = new(cx, cy - r);
        Point end = new(
            cx + r * Math.Cos(endAngle * Math.PI / 180.0),
            cy + r * Math.Sin(endAngle * Math.PI / 180.0));

        var fig = new PathFigure { StartPoint = start, IsClosed = false };
        fig.Segments.Add(new ArcSegment
        {
            Size = new Size(r, r),
            Point = end,
            IsLargeArc = sweep >= 180,
            SweepDirection = SweepDirection.Clockwise
        });

        var geo = new PathGeometry();
        geo.Figures.Add(fig);
        ProgressArc.Data = geo;
    }
}
