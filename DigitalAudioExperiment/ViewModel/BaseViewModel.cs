/*
    Digital Audio Experiement: Plays mp3 files and may be others in the future.
    Copyright (C) 2024  Michael Chand

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

        /// <summary>
        /// Dispose by application control. 
        /// </summary>
        /// <remarks>
        /// Use this when you want the ViewModel to persist until your application logic
        /// explicitely needs to dispose. Eg Persist ViewModel resources after its associated
        /// dialog or window has closed.
        /// </remarks>
        public virtual void ExplicitDispose()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
