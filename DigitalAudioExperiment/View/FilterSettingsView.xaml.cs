using DigitalAudioExperiment.ViewModel;
using System.Windows;
using System.Windows.Input;

namespace DigitalAudioExperiment.View
{
    public partial class FilterSettingsView : Window, IDisposable
    {
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
                viewModel.SetExitCallback(ExitSettings);
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
    }
}
