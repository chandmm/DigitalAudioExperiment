using DigitalAudioExperiment.ViewModel;
using DigitalAudioExperiment.ViewModel.SettingsViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DigitalAudioExperiment.View
{
    public partial class FilterSettingsView : Window, IDisposable
    {
        public static FilterSettingsView Instance { get; private set; }

        private bool _isDisposed;

        public FilterSettingsView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.DragMove();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is FilterSettingsViewModel viewModel)
            {
                Instance = this;

                this.Resources.MergedDictionaries.Add(App.Current.Resources.MergedDictionaries.First());
                viewModel.SetExitCallback(ExitSettings);
                SettingsViewModel.GetSettingsInstance(null, null, null)?.ApplyCurrentTheme();
            }
        }

        private void ExitSettings()
        {
            Dispose();
            this.Close();
        }

        public bool IsDisposed()
            => _isDisposed;

        private void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    DataContextChanged -= OnDataContextChanged;
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void SliderSetting_MouseWheel(object sender, MouseWheelEventArgs args)
        {
            if (DataContext is FilterSettingsViewModel viewModel)
            {
                if (sender is Slider slider)
                {
                    switch (slider.Name)
                    {
                        case "cutoffFrequencySetting":
                            viewModel.CutoffFrequency = UpdateSettingsFromSliderMousewheel(args.Delta, (int)cutoffFrequencySetting.Maximum, (int)cutoffFrequencySetting.Minimum, viewModel.CutoffFrequency);
                            break;
                        case "bandwidthSetting":
                            viewModel.Bandwidth = UpdateSettingsFromSliderMousewheel(args.Delta, (int)bandwidthSetting.Maximum, (int)bandwidthSetting.Minimum, viewModel.Bandwidth);
                            break;
                        case "filterOrderSetting":
                            viewModel.FilterOrder = UpdateSettingsFromSliderMousewheel(args.Delta, (int)filterOrderSetting.Maximum, (int)filterOrderSetting.Minimum, viewModel.FilterOrder, 2);
                            break;
                        default:
                            args.Handled = true;
                            return;
                    }
                }

                args.Handled = true;
            }
        }

        private int UpdateSettingsFromSliderMousewheel(int delta, int max, int min, int value, int multiplier = 1)
        {
            if (delta > 0)
            {
                value = (value + 1) > max ? max : value + multiplier;
            }

            if (delta < 0)
            {
                value = (value - 1) < min ? min : value - multiplier;
            }

            return value;
        }
    }
}
