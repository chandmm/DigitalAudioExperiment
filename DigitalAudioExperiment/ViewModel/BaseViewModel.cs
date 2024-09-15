using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DigitalAudioExperiment.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool _isDisposed = false;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected void OnPropertyChanged(params string[] propertyNames)
        {
            if (propertyNames == null
                || !propertyNames.Any())
            {
                return;
            }

            foreach (var name in propertyNames)
            {
                OnPropertyChanged(name);
            }
        }

        protected abstract void Dispose(bool isDisposng);

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
