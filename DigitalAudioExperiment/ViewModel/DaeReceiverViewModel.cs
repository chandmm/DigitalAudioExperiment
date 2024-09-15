using DigitalAudioExperiment.Infrastructure;

namespace DigitalAudioExperiment.ViewModel
{
    public class DaeReceiverViewModel : BaseViewModel
    {
        #region Fields

        private Func<string?> _getFile;

        #endregion

        #region Properties

        private string _title;
        public string Title
        {
            get => _title;
            set 
            {
                _title = value;

                OnPropertyChanged();
            } 
        }

        private string _subTitle;
        public string SubTitle
        {
            get => _subTitle;
            set
            {
                _subTitle = value;

                OnPropertyChanged();
            }
        }

        private bool _isMono;
        public bool IsMono
        {
            get => _isMono;
        }

        private double _values;
        public double Values
        {
            get => _values;
            set
            {
                _values = value;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public RelayCommand ExitCommand { get; set; }
        public RelayCommand PlayCommand { get; set; }
        public RelayCommand SelectCommand { get; set; }

        #endregion

        #region Initialisation

        public DaeReceiverViewModel()
        {
            Title = "Digital Audio Experiment(DAE)";
            SubTitle = "Mp3 Digital Audio";
            _isMono = true;

            ExitCommand = new RelayCommand(() => Environment.Exit(0), () => true);
            PlayCommand = new RelayCommand(PlayButton, () => true);
            SelectCommand = new RelayCommand(SelectFile, () => true);

            RaisePropertyChangedEvents();
        }

        public DaeReceiverViewModel(Func<string?> callback)
            : this()
        {
            SetGetFileCallback(callback);
        }

        public void SetGetFileCallback(Func<string> callback)
            => _getFile = callback;

        #endregion

        #region Playback Logic

        private void PlayButton()
        {
            //Values += 10d;
        }

        private void SelectFile()
        {
            if (_getFile == null)
            {
                return;
            }

            _getFile.Invoke();
        }

        #endregion

        #region Events

        private void RaisePropertyChangedEvents()
        {
            OnPropertyChanged(nameof(IsMono));
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool isDisposng)
        {
            if (!_isDisposed)
            {
                if (isDisposng)
                {
                    // TODO: Dispose resources.
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
