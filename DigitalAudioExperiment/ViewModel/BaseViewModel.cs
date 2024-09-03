using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DigitalAudioExperiment.ViewModel
{
    public abstract class BaseViewModel : INotifyPropertyChanged, IDisposable
    {
        protected bool _isDisposed;

        public event PropertyChangedEventHandler? PropertyChanged;


        #region Events

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion

        #region Dispose

        protected abstract void Dispose(bool disposing);

        public virtual void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
