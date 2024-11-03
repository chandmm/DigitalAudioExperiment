using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace DigitalAudioExperiment.Behaviours
{
    public static class SeekSliderBehavior
    {
        public static readonly DependencyProperty EnableMoveToPointProperty =
            DependencyProperty.RegisterAttached(
                "EnableMoveToPoint",
                typeof(bool),
                typeof(SeekSliderBehavior),
                new PropertyMetadata(false, OnEnableMoveToPointChanged));

        public static bool GetEnableMoveToPoint(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableMoveToPointProperty);
        }

        public static void SetEnableMoveToPoint(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableMoveToPointProperty, value);
        }

        private static void OnEnableMoveToPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Slider slider)
            {
                if ((bool)e.NewValue)
                {
                    slider.Loaded += Slider_Loaded;
                    slider.Unloaded += Slider_Unloaded;
                }
                else
                {
                    slider.Loaded -= Slider_Loaded;
                    slider.Unloaded -= Slider_Unloaded;
                }
            }
        }

        private static void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Slider slider)
            {
                if (slider.Template.FindName("PART_Track", slider) is Track track)
                {
                    track.MouseLeftButtonDown += Track_MouseLeftButtonDown;
                }
            }
        }

        private static void Slider_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is Slider slider)
            {
                if (slider.Template.FindName("PART_Track", slider) is Track track)
                {
                    track.MouseLeftButtonDown -= Track_MouseLeftButtonDown;
                }
            }
        }

        private static void Track_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Track track)
            {
                var slider = FindParent<Slider>(track);
                if (slider != null)
                {
                    // Get the position of the mouse relative to the track
                    var mousePosition = e.GetPosition(track);

                    // Calculate the new value
                    double newValue = CalculateValueFromMousePosition(slider, mousePosition, track);

                    // Update the slider's value
                    slider.Value = newValue;

                    e.Handled = true;
                }
            }
        }

        private static double CalculateValueFromMousePosition(Slider slider, Point mousePosition, Track track)
        {
            bool isHorizontal = slider.Orientation == Orientation.Horizontal;

            double value = slider.Minimum;

            double thumbLength = isHorizontal ? track.Thumb.ActualWidth : track.Thumb.ActualHeight;
            double trackLength = isHorizontal ? track.ActualWidth - thumbLength : track.ActualHeight - thumbLength;
            double position = isHorizontal ? mousePosition.X - (thumbLength / 2) : mousePosition.Y - (thumbLength / 2);

            double ratio = position / trackLength;

            ratio = Math.Max(0, Math.Min(1, ratio));

            double range = slider.Maximum - slider.Minimum;

            value = slider.Minimum + (ratio * range);

            if (slider.IsDirectionReversed)
            {
                value = slider.Maximum - (ratio * range);
            }

            double adjustedValue = Math.Round(value / slider.SmallChange) * slider.SmallChange;

            return adjustedValue;
        }

        private static T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
            {
                return parent;
            }
            else
            {
                return FindParent<T>(parentObject);
            }
        }
    }

}
