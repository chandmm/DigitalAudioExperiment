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

        public DaeReceiverViewModel()
        {
            Title = "Digital Audio Experiment(DAE)";
            SubTitle = "Mp3 Digital Audio";
        }

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
    }
}
