using DigitalAudioExperiment.Infrastructure;

namespace DigitalAudioExperiment.ViewModel
{
    public class DaeReceiverViewModel : BaseViewModel
    {
        private string _title;
        public string Title
        {
            get => _title;
            set 
            {
                _title = value;

                OnNotifyPropertyChanged();
            } 
        }

        private string _subTitle;
        public string SubTitle
        {
            get => _subTitle;
            set
            {
                _subTitle = value;

                OnNotifyPropertyChanged();
            }
        }


        #region Commands

        public RelayCommand ExitCommand { get; set; }

        #endregion

        public DaeReceiverViewModel()
        {
            Title = "Digital Audio Experiment(DAE)";
            SubTitle = "Mp3 Digital Audio";

            ExitCommand = new RelayCommand(() => Environment.Exit(0), () => true);
        }

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
