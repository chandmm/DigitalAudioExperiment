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
using System.Timers;

namespace DigitalAudioExperiment.ViewModel
{
    public class DaeReceiverViewModel : BaseViewModel
    {
        #region Fields
        private readonly double _tickPercentage = 0.01;
        private readonly int _initialSafeVolume = 10;

        private Func<string?> _getFile;
        private DaeAudioPlayer _player;
        private System.Timers.Timer _vuUpdateTimer;
        private System.Timers.Timer _applicationHeartBeatTimer;
        private double _vuHeartBeatInterval = 50;
        private double _heartBeatInterval = 200;
        private bool _canContinueLoopMode = true;

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

        private int _durationHours;
        public int DurationHours
        {
            get => _durationHours;
            set
            {
                _durationHours = value;

                OnPropertyChanged();
            }
        }

        private int _durationMinutes;
        public int DurationMinutes
        {
            get => _durationMinutes;
            set
            {
                _durationMinutes = value;

                OnPropertyChanged();
            }
        }

        private int _durationSeconds;
        public int DurationSeconds
        {
            get => _durationSeconds;
            set
            {
                _durationSeconds = value;

                OnPropertyChanged();
            }
        }

        private int _elapsedHours;
        public int ElapsedHours
        {
            get => _elapsedHours;
            set
            {
                _elapsedHours = value;

                OnPropertyChanged();
            }
        }

        private int _elapsedMinutes;
        public int ElapsedMinutes
        {
            get => _elapsedMinutes;
            set
            {
                _elapsedMinutes = value;

                OnPropertyChanged();
            }
        }

        private int _elapsedSeconds;
        public int ElapsedSeconds
        {
            get => _elapsedSeconds;
            set
            {
                _elapsedSeconds = value;

                OnPropertyChanged();
            }
        }

        private int _bitRate;
        public int Bitrate
        {
            get => _bitRate;
            set
            {
                _bitRate = value;

                OnPropertyChanged();
            }
        }

        public string HeaderData { get; private set; }
        public string Metadata { get; private set; }

        private double _leftdB;
        public double LeftdB
        {
            get => _leftdB;
            set
            {
                _leftdB = value;

                OnPropertyChanged();
            }
        }

        private double _rightdB;
        public double RightdB
        {
            get => _rightdB;
            set
            {
                _rightdB = value;

                OnPropertyChanged();
            }
        }

        private bool _isAutoPlayChecked;
        public bool IsAutoPlayChecked
        {
            get => _isAutoPlayChecked;
            set
            {
                _isAutoPlayChecked = value;

                OnPropertyChanged();
            }
        }

        private bool _isLoopPlayChecked;
        public bool IsLoopPlayChecked
        {
            get => _isLoopPlayChecked;
            set
            {
                _isLoopPlayChecked = value;
                _canContinueLoopMode = IsLoopPlayChecked;

                OnPropertyChanged();
            }
        }

        private bool _isOn;
        public bool IsOn
        {
            get => _isOn;
            set
            {
                _isOn = value;
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
        public RelayCommand SkipToStartCommand { get; set; }

        #endregion

        #region Initialisation

        public DaeReceiverViewModel()
        {
            Title = "Digital Audio Experiment(DAE)";
            SubTitle = "Mp3 Digital Audio";
            _isMono = true;

            ExitCommand = new RelayCommand(() => Exit(), () => true);
            PlayCommand = new RelayCommand(async () => await PlayButton(), () => true);
            PauseCommand = new RelayCommand(async () => await PauseButton(), () => true);
            SelectCommand = new RelayCommand(SelectFile, () => true);
            StopCommand = new RelayCommand(StopButton, () => true);
            SkipToStartCommand = new RelayCommand(SkipToStartButton, () => true);

            Volume = _initialSafeVolume;
            VolumeLabel = "Volume";
            IsAutoPlayChecked = true;
            _vuUpdateTimer = new System.Timers.Timer(_vuHeartBeatInterval);
            _vuUpdateTimer.AutoReset = true;
            _vuUpdateTimer.Elapsed += UpdateVuMeterControls;

            _applicationHeartBeatTimer = new System.Timers.Timer(_heartBeatInterval);
            _applicationHeartBeatTimer.AutoReset = true;
            _applicationHeartBeatTimer.Elapsed += UpdateApplicationHeartBeat;
            _applicationHeartBeatTimer.Start();

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
            _canContinueLoopMode = false;
            _player.Stop();
            _vuUpdateTimer.Stop();
        }

        private async Task PlayButton()
        {
            if (!_vuUpdateTimer.Enabled)
            {
                _vuUpdateTimer.Start();
            }

            _canContinueLoopMode = IsLoopPlayChecked;

            await Task.Run(() =>
            {
                _player?.Play();

            }).ConfigureAwait(false);

            RaisePropertyChangedEvents();
        }

        private async Task PauseButton()
        {
            _player?.Pause();
        }

        private async void SkipToStartButton()
        {
            Value = 0;
            SetSeekValue();
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
            _player.SetUpdateCallback(Update);
            Maximum = _player.GetFrameCount() ?? 0;
            _isMono = _player.GetIsMonoChannel();
            Value = 0;
            _player.SetVolume(Volume);
            DurationMinutes = _player.Duration().Item1;
            DurationSeconds = _player.Duration().Item2;
            DurationHours = DurationMinutes / 60;
            HeaderData = _player.GetAudioFileInfo();
            Metadata = _player?.GetMetadata();

            SetTickFrequency();

            RaisePropertyChangedEvents();

            if (IsAutoPlayChecked)
            {
                PlayButton();
            }
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
        private void Update()
        {
            if (_player.IsStopped
                && !IsLoopPlayChecked)
            {
                _vuUpdateTimer.Stop();
                LeftdB = 0;
                RightdB = 0;

                return;
            }

            if (IsLoopPlayChecked
                && _canContinueLoopMode)
            {
                PlayButton();
            }

            var duration = _player?.GetElapsed();

            ElapsedMinutes = (int)(duration / 60);
            ElapsedSeconds = (int)(duration % 60);
            ElapsedHours = (ElapsedMinutes / 60);

            Bitrate = (int)_player?.GetBitratePerFrame();
        }

        private void Exit()
        {
            this.Dispose();
            
            Environment.Exit(0);
        }

        #endregion

        #region Callbacks

        private void UpdateVuMeterControls(object? sender, ElapsedEventArgs e)
        {
            if (_player == null
                || _player.IsStopped)
            {
                return;
            }

            var levels = _player?.GetVUMeterValues();

            LeftdB = levels.Value.Item1;
            RightdB = levels.Value.Item2;
        }

        private void UpdateApplicationHeartBeat(object? sender, ElapsedEventArgs e)
        {
            IsOn = _player == null ? false : !_player.IsStopped;
        }

        #endregion

        #region Events

        private void RaisePropertyChangedEvents()
        {
            OnPropertyChanged(nameof(IsMono)
                , nameof(HeaderData)
                , nameof(Metadata));
        }

        #endregion

        #region Dispose

        protected override void Dispose(bool isDisposng)
        {
            if (!_isDisposed)
            {
                if (isDisposng)
                {
                    if (_vuUpdateTimer.Enabled)
                    {
                        _vuUpdateTimer.Stop();
                        _vuUpdateTimer?.Dispose();
                    }

                    if (_applicationHeartBeatTimer.Enabled)
                    {
                        _applicationHeartBeatTimer.Stop();
                        _applicationHeartBeatTimer?.Dispose();
                    }

                    _player?.Dispose();
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
