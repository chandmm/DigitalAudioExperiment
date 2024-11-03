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
using DigitalAudioExperiment.View;
using System.IO;
using System.Timers;

namespace DigitalAudioExperiment.ViewModel
{
    public class ReceiverViewModel : BaseViewModel
    {
        #region Fields

        private readonly double _tickPercentage = 0.01;
        private readonly int _initialSafeVolume = 5;

        private Func<string?, string> _getFile;
        private IAudioPlayer _player;
        private System.Timers.Timer _vuUpdateTimer;
        private System.Timers.Timer _applicationHeartBeatTimer;
        private double _vuHeartBeatInterval = 10;
        private bool _canContinueLoopMode = true;
        private PlaylistPageView _playlistPageView;
        private FilterSettingsViewModel _filterSettingsViewModel;
        private FilterSettingsView _filterSettingsView;
        private bool _isSeeking;

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

        private double _seekIndicatorValue;
        public double SeekIndicatorValue
        {
            get => _seekIndicatorValue;
            set
            {
                _seekIndicatorValue = value;

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
        private double _minimum;
        public double Minimum
        {
            get => _minimum;
            set
            {
                _minimum = value;

                OnPropertyChanged();
            }
        }

        private double _sliderMaximum = 100;
        public double SliderMaximum
        {
            get => _sliderMaximum;
            set
            {
                _sliderMaximum = value;

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

        private string _vuLabel;
        public string VuLabel
        {
            get => _vuLabel;
            set
            {
                _vuLabel = value;

                OnPropertyChanged();
            }
        }

        public string DecoderType { get; set; }

        #endregion

        #region Commands

        public RelayCommand ExitCommand { get; set; }
        public RelayCommand PlayCommand { get; set; }
        public RelayCommand PauseCommand { get; set; }
        public RelayCommand SelectCommand { get; set; }
        public RelayCommand StopCommand { get; set; }
        public RelayCommand SkipToStartCommand { get; set; }
        public RelayCommand SkipToEndCommand { get; set; }
        public RelayCommand OpenPlaylistCommand { get; set; }
        public RelayCommand OpenVisualisationFilterSettingsCommand { get; set; }
        public RelayCommand SetAutoplayModeToggleCommand { get; set; }
        public RelayCommand SetLoopPlayModeToggleCommand { get; set; }

        #endregion

        #region Initialisation

        public ReceiverViewModel()
        {
            Title = "Digital Audio Experiment(DAE)";
            SubTitle = "Mp3 Digital Audio";
            VuLabel = "DB RMS Power";
            VolumeLabel = "Volume";
            _isMono = true;

            ExitCommand = new RelayCommand(() => Exit(), () => true);
            PlayCommand = new RelayCommand(async () => PlayButtonFromCommand(), () => true);
            PauseCommand = new RelayCommand(async () => await PauseButton(), () => true);
            SelectCommand = new RelayCommand(SelectFile, () => true);
            StopCommand = new RelayCommand(StopButton, () => true);
            SkipToStartCommand = new RelayCommand(SkipToStartButton, () => true);
            SkipToEndCommand = new RelayCommand(SkipToEndButton, () => true);
            OpenPlaylistCommand = new RelayCommand(OpenPlaylist, () => true);
            OpenVisualisationFilterSettingsCommand = new RelayCommand(OpenVisualisationFilterSettings, () => true);
            SetAutoplayModeToggleCommand = new RelayCommand(SetAutoplayModeToggle, () => true);
            SetLoopPlayModeToggleCommand = new RelayCommand(SetLoopPlayModeToggle, () => true);

            Volume = _initialSafeVolume;
            IsAutoPlayChecked = true;
            _vuUpdateTimer = new System.Timers.Timer(_vuHeartBeatInterval);
            _vuUpdateTimer.AutoReset = true;
            _vuUpdateTimer.Elapsed += UpdateVuMeterControls;
            SliderMaximum = 1;
            Maximum = 100;
            Minimum = 0;
            SeekIndicatorValue = 0;

            _filterSettingsView = new FilterSettingsView();
            _filterSettingsView.DataContext = _filterSettingsViewModel = new FilterSettingsViewModel();
            _filterSettingsViewModel.OnSettingsApplied += OnFilterSettingsApplied;

            RaisePropertyChangedEvents();
        }

        public void SetGetFileCallback(Func<string, string> callback)
            => _getFile = callback;

        #endregion

        #region Playback Logic

        private void StopButton()
        {
            _canContinueLoopMode = false;
            _player?.Stop();
            _vuUpdateTimer.Stop();
            _player?.SetHardStop(true);
            _player?.Dispose();
            _player = null;

            SeekIndicatorValue = 0;
            LeftdB = Minimum;
            RightdB = Minimum;
        }

        private async void PlayButtonFromCommand()
        {
            await PlayInternal(true);
        }

        private async Task PlayInternal(bool fromPlayButton = false)
        {
            if (_player == null
                && _playlistPageView == null)
            {
                return;
            }

            if (_player != null
                && !_player.IsStopped()
                && _player.IsDisposed())
            {
                _player.Stop();
                _player.Dispose();
                _player = null;
            }

            if (_player == null
                && _playlistPageView.DataContext is PlaylistPageViewModel newViewModel
                && newViewModel != null
                && newViewModel.IsHasList)
            {
                ResetPlayer();
                SetupWithAutoPlay(autoPlayOverride: true, fromPlayButton);
            }

            if (_player == null)
            {
                return;
            }

            if (!_vuUpdateTimer.Enabled)
            {
                _vuUpdateTimer.Start();
            }

            _canContinueLoopMode = IsLoopPlayChecked;

            _player.SetHardStop(false);

            OnFilterSettingsApplied(_filterSettingsViewModel);

            Play();
        }

        private void Play()
        {
            Task.Run(() =>
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
            SeekIndicatorValue = 0;
            SetSeekValue();
        }

        private void SkipToEndButton()
        {
            SeekIndicatorValue = (int)_player?.GetFrameCount() - 1;
            SetSeekValue();
        }

        private async void SelectFile()
        {
            if (_getFile == null)
            {
                return;
            }

            var fileName = _getFile.Invoke(PlaylistPageViewModel.FileFiltersAudio);

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            if (_playlistPageView != null
                && _playlistPageView.DataContext is PlaylistPageViewModel viewModel)
            {
                viewModel.RemoveAll();
            }

            OnFileSelected(fileName);
        }

        private void ResetPlayer()
        {
            if (_player != null)
            {
                _player.Stop();
                _player.Dispose();
            }

            SeekIndicatorValue = 0;
        }

        private void OnFileSelected(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            SetupPlayListControls(fileName);

            if (_playlistPageView.DataContext is PlaylistPageViewModel playlistPageViewModel
                && playlistPageViewModel.PlayList.Count() == 1
                && _player == null)
            {
                ResetPlayer();
                SetupWithAutoPlay();
            }
        }

        private void SetupPlayListControls(string fileName)
        {
            if (_playlistPageView == null)
            {
                _playlistPageView = new PlaylistPageView();
                _playlistPageView.DataContext = new PlaylistPageViewModel();
            }

            if (_playlistPageView.DataContext is PlaylistPageViewModel playlistPageViewModel)
            {
                playlistPageViewModel.Add(fileName);
            }
        }

        private void SetupWithAutoPlay(bool autoPlayOverride = false, bool fromPlayButton = false)
        {
            if (!(_playlistPageView.DataContext is PlaylistPageViewModel viewModel))
            {
                return;
            }

            var fileName = viewModel.GetNextFile();

            if (string.IsNullOrEmpty(fileName)
                && fromPlayButton)
            {
                viewModel.ResetToSelectedPlayed();

                fileName = viewModel.GetNextFile();
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            try
            {
                _player = AudioPlayerFactory.GetAudioPlayerInterface(fileName);
                _player.SetSeekPositionCallback(UpdatePosition);
                _player.SetUpdateCallback(Update);
                _player.SetPlaybackStoppedCallback(PlaybackStoppedCallback);
                SliderMaximum = _player.GetFrameCount() ?? 0;
                _isMono = _player.GetIsMonoChannel();
                SeekIndicatorValue = 0;
                _player.SetVolume(Volume);
                DurationMinutes = _player.Duration().Item1;
                DurationSeconds = _player.Duration().Item2;
                DurationHours = DurationMinutes / 60;
                HeaderData = _player.GetAudioFileInfo();
                Metadata = _player?.GetMetadata();

                SetTickFrequency();

                RaisePropertyChangedEvents();

                if (IsAutoPlayChecked
                    && !autoPlayOverride)
                {
                    PlayInternal();
                }
            }
            catch (FileNotFoundException fileNotFoundException)
            {
                _ = fileNotFoundException;

                _player?.Dispose();
                _player = null;

                if (_playlistPageView != null
                    && viewModel != null
                    && viewModel.IsHasList)
                {
                    SetupWithAutoPlay(autoPlayOverride, fromPlayButton);
                }
            }

            DecoderType = _player?.DecoderType;
            OnPropertyChanged(nameof(DecoderType));
        }

        private void SetTickFrequency()
        {
            TickFrequency = SliderMaximum * _tickPercentage;
        }

        private void UpdatePosition(int position)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (!_isSeeking)
                {
                    SeekIndicatorValue = position;
                }
            });
        }

        public void StartIsSeeking(bool isSeeking)
        {
            _isSeeking = isSeeking;
        }

        public void SetSeekValue()
        {
            _player?.Seek((int)SeekIndicatorValue);
        }

        private void SetAutoplayModeToggle()
        {
            IsAutoPlayChecked = !IsAutoPlayChecked;
        }

        private void SetLoopPlayModeToggle()
        {
            IsLoopPlayChecked = !IsLoopPlayChecked;
        }

        #endregion

        #region Application Logic

        private void OpenPlaylist()
        {
            if (_playlistPageView != null
                && (_playlistPageView.DataContext is PlaylistPageViewModel viewModel)
                && viewModel.IsShowing)
            {
                _playlistPageView.Close();

                return;
            }

            if (_playlistPageView == null)
            {
                _playlistPageView = new PlaylistPageView();
                _playlistPageView.Owner = App.Current.MainWindow;
            }

            _playlistPageView.DataContext = _playlistPageView.DataContext == null || (_playlistPageView.DataContext is PlaylistPageViewModel) == null
                ? new PlaylistPageViewModel()
                : _playlistPageView.DataContext;
            _playlistPageView.Show();
        }

        private void VolumeAdjust(int value)
        {
            _volume = value;
            _player?.SetVolume(_volume);
        }

        private void Update()
        {
            if (_player == null)
            {
                return;
            }

            if (_player.IsStopped()
                && !IsLoopPlayChecked)
            {
                _vuUpdateTimer.Stop();
                LeftdB = 0;
                RightdB = 0;

                return;
            }

            var duration = _player?.GetElapsed();

            ElapsedMinutes = (int)(duration / 60);
            ElapsedSeconds = (int)(duration % 60);
            ElapsedHours = (ElapsedMinutes / 60);

            Bitrate = (int)_player?.GetBitratePerFrame();
        }

        private void OpenVisualisationFilterSettings()
        {
            if (_filterSettingsView.IsDisposed())
            {
                _filterSettingsView = new FilterSettingsView();
                _filterSettingsView.DataContext = _filterSettingsViewModel;
            }

            if (_filterSettingsView.IsLoaded)
            {
                return;
            }

            _filterSettingsView.Show();
        }

        private void Exit()
        {
            this.Dispose();
            
            Environment.Exit(0);
        }

        #endregion

        #region Callbacks

        private void OnFilterSettingsApplied(FilterSettingsViewModel filterSettingsViewModel)
        {
            _player?.UpdateFilterSettings(filterSettingsViewModel);
        }

        private void UpdateVuMeterControls(object? sender, ElapsedEventArgs e)
        {
            if (_player == null
                || _player.IsStopped())
            {
                return;
            }

            if (!IsOn)
            {
                IsOn = true;
            }

            //var levels = _player?.GetVUMeterValues();
            var levels = _player?.GetDbVuValues();
            
            LeftdB = levels.Value.left;
            RightdB = levels.Value.right;
        }

        private void PlaybackStoppedCallback()
        {
            IsOn = false;

            if (IsLoopPlayChecked
                && _canContinueLoopMode
                && _player.IsStopped())
            {
                PlayInternal();

                return;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                if (IsAutoPlayChecked
                    && _player != null
                    && !_player.IsHardStop()
                    && _playlistPageView.DataContext is PlaylistPageViewModel viewModel
                    && !viewModel.IsLastItem())
                {
                    ResetPlayer();
                    SetupWithAutoPlay();
                }
                else if (_player != null
                        && _playlistPageView.DataContext is PlaylistPageViewModel playListViewModel
                        && playListViewModel.IsLastItem()
                        && _player.IsStopped())
                {
                    StopButton();
                    playListViewModel.ResetToSelectedPlayed();
                }
            });
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

                    _player?.Dispose();

                    _playlistPageView?.CloseExit();
                    
                    if (_playlistPageView?.DataContext is PlaylistPageViewModel viewModel)
                    {
                        viewModel.Dispose();
                        _playlistPageView.DataContext = null;
                    }

                    _filterSettingsViewModel.OnSettingsApplied -= OnFilterSettingsApplied;
                    _filterSettingsViewModel?.Dispose();
                    _filterSettingsView?.Dispose();
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
