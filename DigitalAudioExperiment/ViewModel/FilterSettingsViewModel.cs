using DigitalAudioExperiment.Infrastructure;

namespace DigitalAudioExperiment.ViewModel
{
    public class FilterSettingsViewModel : BaseViewModel
    {
        #region Fields
        private bool _isDisposed;
        private Action _exitSettingsCallback;

        public delegate void ApplySettingsEvent(float cutoff, float bandwidth, int filterOrder);
        public event ApplySettingsEvent OnSettingsApplied;

        #endregion

        #region Properties

        private int _cutoffFrequency;
        public int CutoffFrequency
        {
            get => _cutoffFrequency;
            set
            {
                _cutoffFrequency = value;

                OnPropertyChanged();
            }
        }

        private int _bandwidth;
        public int Bandwidth
        {
            get => _bandwidth;
            set
            {
                _bandwidth = value;

                OnPropertyChanged();
            }
        }

        private int _filterOrder;
        public int FilterOrder
        {
            get => _filterOrder;
            set
            {
                _filterOrder = value;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public RelayCommand ExitCommand { get; set; }

        #endregion

        #region Initialisation
        
        public FilterSettingsViewModel()
        {
            ExitCommand = new RelayCommand(ExitFilterSettings, () => true);    
        }

        #endregion

        #region Application Logic

        private void ExitFilterSettings()
        {
            _exitSettingsCallback?.Invoke();
        }

        #endregion


        #region Dispose

        protected override void Dispose(bool isDisposng)
        {
            if (!_isDisposed)
            {
                if (isDisposng)
                {
                    // Dispose managed resources.
                }

                _isDisposed = true;
            }
        }

        public void SetExitCallback(Action exitSettings)
        {
            _exitSettingsCallback = exitSettings;
        }

        #endregion
    }
}
