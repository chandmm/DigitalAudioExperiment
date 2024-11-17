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
using DigitalAudioExperiment.Infrastructure;
using DigitalAudioExperiment.Model;
using System.IO;
using System.Windows;

namespace DigitalAudioExperiment.ViewModel
{
    public class SettingsViewModel : BaseViewModel
    {
        private ReceiverViewModel _receiver;
        private FilterSettingsViewModel _filterSettingsViewModel;

        public bool IsReset { get; private set; }

        public RelayCommand ResetToDefaultCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }

        public SettingsViewModel(ReceiverViewModel receiver, FilterSettingsViewModel filterSettingsViewModel, Action windowCloseFunction)
        {
            _receiver = receiver;
            _filterSettingsViewModel = filterSettingsViewModel;

            ResetToDefaultCommand = new RelayCommand(ResetToDefault, () => true);
            CloseCommand = new RelayCommand(() => windowCloseFunction?.Invoke(), () => true);
        }

        private void ResetToDefault()
        {
            var result = MessageBox.Show("This will reset player settings to default. You will lose your saved player settings. Are you sure?"
                , "Reset Settings"
                , MessageBoxButton.YesNo
                , MessageBoxImage.Warning);

            IsReset = result == MessageBoxResult.Yes;

            if (result == MessageBoxResult.No)
            {
                return;
            }

            if (File.Exists(Settings.SettingsFilePath))
            {
                File.Delete(Settings.SettingsFilePath);
            }

            Settings.CreateIfNotExists(_receiver, _filterSettingsViewModel);
        }

        protected override void Dispose(bool isDisposng)
        {
            if (!_isDisposed)
            {
                if (isDisposng)
                {
                    _receiver = null;
                    _filterSettingsViewModel = null;
                }

                _isDisposed = true;
            }
        }
    }
}
