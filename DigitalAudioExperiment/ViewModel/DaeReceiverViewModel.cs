/*
    Digital Audio Experiement: Plays mp3 files and may be others in the future.
    Copyright (C) 2024  Michael Chand.

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
using DigitalAudioExperiment.Logic;
using System.Diagnostics;

namespace DigitalAudioExperiment.ViewModel
{
    public class DaeReceiverViewModel : BaseViewModel
    {
        #region Fields
        private readonly double _tickPercentage = 0.01;

        private Func<string?> _getFile;
        private DaeAudioPlayer _player;

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

        private double _value;
        public double Value
        {
            get => _value;
            set
            {
                _value = value;

                OnPropertyChanged();
            }
        }

        private double _maximum;
        public double Maximum
        {
            get => _maximum;
            set
            {
                _maximum = value;

                OnPropertyChanged();
            }
        }

        private double _tickFrequency;
        public double TickFrequency
        {
            get => _tickFrequency;
            set
            {
                _tickFrequency = value;

                OnPropertyChanged();
            }
        }

        private int _volume;
        public int Volume
        {
            get => _volume;
            set
            {
                VolumeAdjust(value);

                OnPropertyChanged();
            }
        }

        private string _volumeLabel;
        public string VolumeLabel
        {
            get => _volumeLabel;
            set
            {
                _volumeLabel = value;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public RelayCommand ExitCommand { get; set; }
        public RelayCommand PlayCommand { get; set; }
        public RelayCommand PauseCommand { get; set; }
        public RelayCommand SelectCommand { get; set; }
        public RelayCommand StopCommand { get; set; }

        #endregion

        #region Initialisation

        public DaeReceiverViewModel()
        {
            Title = "Digital Audio Experiment(DAE)";
            SubTitle = "Mp3 Digital Audio";
            _isMono = true;

            ExitCommand = new RelayCommand(() => Environment.Exit(0), () => true);
            PlayCommand = new RelayCommand(async () => await PlayButton(), () => true);
            PauseCommand = new RelayCommand(async () => await PauseButton(), () => true);
            SelectCommand = new RelayCommand(SelectFile, () => true);
            StopCommand = new RelayCommand(StopButton, () => true);
            Volume = 20;
            VolumeLabel = "Vol";
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

        private void StopButton()
        {
            _player.Stop();
        }

        private async Task PlayButton()
        {
            await Task.Run(() => _player.Play()).ConfigureAwait(false);
        }

        private async Task PauseButton()
        {
            _player?.Pause();
        }

        private async void SelectFile()
        {
            if (_getFile == null)
            {
                return;
            }

            var fileName = _getFile.Invoke();

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            OnFileSelected(fileName);
        }

        private void OnFileSelected(string fileName)
        {
            if (_player != null)
            {
                _player.Stop();
                _player.Dispose();
            }

            _player = new DaeAudioPlayer(fileName);
            _player.SetSeekPositionCallback(UpdatePosition);
            Maximum = _player.GetFrameCount() ?? 0;
            _isMono = _player.GetIsMonoChannel();
            Value = 0;
            _player.SetVolume(Volume);

            SetTickFrequency();

            RaisePropertyChangedEvents();
        }

        private void SetTickFrequency()
        {
            TickFrequency = Maximum * _tickPercentage;
        }

        private void UpdatePosition(int position)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                Value = position;
            });
        }

        public void StartIsSeeking(bool isSeeking)
        {
            _player?.Pause();
        }

        public void SetSeekValue()
        {
            _player?.Seek((int)Value);
        }

        #endregion

        #region Application Logic

        private void VolumeAdjust(int value)
        {
            _volume = value;
            _player?.SetVolume(_volume);
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
                    _player?.Dispose();
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
