using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DigitalAudioExperiment.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool _isDisposed = false;

        protected void OnNotifyPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected abstract void Dispose(bool isDisposng);

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
