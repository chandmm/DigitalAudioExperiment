/*
    Digital Audio Experiement: Plays mp3 files and may be others in the future.
    Copyright (C) 2024  Michael Chand.

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace DigitalAudioExperiment.Behaviours
{
    public class SliderBehaviour
    {
        public static readonly DependencyProperty UpdateValueOnDragCompletedProperty =
        DependencyProperty.RegisterAttached(
            "UpdateValueOnDragCompleted",
            typeof(bool),
            typeof(SliderBehaviour),
            new PropertyMetadata(false, OnUpdateValueOnDragCompletedChanged));

        public static bool GetUpdateValueOnDragCompleted(DependencyObject obj)
        {
            return (bool)obj.GetValue(UpdateValueOnDragCompletedProperty);
        }

        public static void SetUpdateValueOnDragCompleted(DependencyObject obj, bool value)
        {
            obj.SetValue(UpdateValueOnDragCompletedProperty, value);
        }

        private static void OnUpdateValueOnDragCompletedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Slider slider)
            {
                if ((bool)e.NewValue)
                {
                    slider.Loaded += Slider_Loaded;
                }
                else
                {
                    slider.Loaded -= Slider_Loaded;
                }
            }
        }

        private static void Slider_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Slider slider)
            {
                var thumb = GetThumb(slider);
                if (thumb != null)
                {
                    thumb.DragCompleted += Thumb_DragCompleted;
                }
            }
        }

        private static void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sender is Thumb thumb && thumb.TemplatedParent is Slider slider)
            {
                var binding = slider.GetBindingExpression(Slider.ValueProperty);
                binding?.UpdateSource();
            }
        }

        private static Thumb GetThumb(Slider slider)
        {
            return FindVisualChild<Thumb>(slider);
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            T child = null;
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            for (int i = 0; i < numVisuals; i++)
            {
                var v = VisualTreeHelper.GetChild(parent, i);

                child = v as T ?? FindVisualChild<T>(v);

                if (child != null)
                {
                    break;
                }
            }
            return child;
        }
    }
}
