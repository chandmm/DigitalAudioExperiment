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
using DigitalAudioExperiment.Model;
using DigitalAudioExperiment.View;
using DigitalAudioExperiment.ViewModel.SettingsViewModels;
using System.ComponentModel;
using System.IO;
using System.Timers;
using System.Windows.Media.Imaging;

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
        private double _vuHeartBeatInterval = 3;
        private bool _canContinueLoopMode = true;
        private FilterSettingsView _filterSettingsView;
        private bool _isSeeking;
        private bool _isInitialising;

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

        private PlaylistPageView _playlistPageView;
        public PlaylistPageView PlaylistPageViewInstance
        {
            get => _playlistPageView;
            set
            {
                _playlistPageView = value;

                OnPropertyChanged();
            }
        }

        private int _bass;
        public int Bass
        {
            get => _bass;
            set
            {
                _bass = value;

                UpdateBassTrebleSettings();

                OnPropertyChanged();
            }
        }

        private int _treble;
        public int Treble
        {
            get => _treble;
            set
            {
                _treble = value;

                UpdateBassTrebleSettings();

                OnPropertyChanged();
            }
        }

        public int BassTrebleRangeMax => 20;
        public int BassTrebleRangeMin => -20;
        public FilterSettingsViewModel FilterSettingsViewModel { get; private set; }

        #endregion

        #region Styling Properties

        public string _backgroundColour;
        public string BackgroundColour
        {
            get => _backgroundColour;
            set
            {
                _backgroundColour = value;

                OnPropertyChanged();
            }
        }

        private string _needleColour;
        public string NeedleColour
        {
            get => _needleColour;
            set
            {
                _needleColour = value;

                OnPropertyChanged();
            }
        }

        private string _decalColour;
        public string DecalColour
        {
            get => _decalColour;
            set
            {
                _decalColour = value;

                OnPropertyChanged();
            }
        }

        private string _overdriveLampColour;
        public string OverdriveLampColour
        {
            get => _overdriveLampColour;
            set
            {
                _overdriveLampColour = value;

                OnPropertyChanged();
            }
        }

        private string _overdriveLampOffColour;
        public string OverdriveLampOffColour
        {
            get => _overdriveLampOffColour;
            set
            {
                _overdriveLampOffColour = value;

                OnPropertyChanged();
            }
        }

        private string _meterLabelForeground;
        public string MeterLabelForeground
        {
            get => _meterLabelForeground;
            set
            {
                _meterLabelForeground = value;

                OnPropertyChanged();
            }
        }

        private string _bottomCoverFill;
        public string BottomCoverFill
        {
            get => _bottomCoverFill;
            set
            {
                _bottomCoverFill = value;

                OnPropertyChanged();
            }
        }

        private string _glowOverlayColour;
        public string SliderThumbGlowOverlay
        {
            get => _glowOverlayColour;
            set
            {
                _glowOverlayColour = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbGripBarBackground;
        public string SliderThumbGripBarBackground
        {
            get => _sliderThumbGripBarBackground;
            set
            {
                _sliderThumbGripBarBackground = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbPointBackground;
        public string SliderThumbPointBackground
        {
            get => _sliderThumbPointBackground;
            set
            {
                _sliderThumbPointBackground = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbBorder;
        public string SliderThumbBorder
        {
            get => _sliderThumbBorder;
            set
            {
                _sliderThumbBorder = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbForeground;
        public string SliderThumbForeground
        {
            get => _sliderThumbForeground;
            set
            {
                _sliderThumbForeground = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbMouseOverBackground;
        public string SliderThumbMouseOverBackground
        {
            get => _sliderThumbMouseOverBackground;
            set
            {
                _sliderThumbMouseOverBackground = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbMouseOverBorder;
        public string SliderThumbMouseOverBorder
        {
            get => _sliderThumbMouseOverBorder;
            set
            {
                _sliderThumbMouseOverBorder = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbPressedBackground;
        public string SliderThumbPressedBackground
        {
            get => _sliderThumbPressedBackground;
            set
            {
                _sliderThumbPressedBackground = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbPressedBorder;
        public string SliderThumbPressedBorder
        {
            get => _sliderThumbPressedBorder;
            set
            {
                _sliderThumbPressedBorder = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbDisabledBackground;
        public string SliderThumbDisabledBackground
        {
            get => _sliderThumbDisabledBackground;
            set
            {
                _sliderThumbDisabledBackground = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbDisabledBorder;
        public string SliderThumbDisabledBorder
        {
            get => _sliderThumbDisabledBorder;
            set
            {
                _sliderThumbDisabledBorder = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbTrackBackground;
        public string SliderThumbTrackBackground
        {
            get => _sliderThumbTrackBackground;
            set
            {
                _sliderThumbTrackBackground = value;

                OnPropertyChanged();
            }
        }

        private string _sliderThumbTrackBorder;
        public string SliderThumbTrackBorder
        {
            get => _sliderThumbTrackBorder;
            set
            {
                _sliderThumbTrackBorder = value;

                OnPropertyChanged();
            }
        }

        private string _skipToStartButtonFill;
        public string SkipToStartButtonFill
        {
            get => _skipToStartButtonFill;
            set
            {
                _skipToStartButtonFill = value;

                OnPropertyChanged();
            }
        }

        private string _stopButtonFill;
        public string StopButtonFill
        {
            get => _stopButtonFill;
            set
            {
                _stopButtonFill = value;

                OnPropertyChanged();
            }
        }

        private string _playButtonFill;
        public string PlayButtonFill
        {
            get => _playButtonFill;
            set
            {
                _playButtonFill = value;

                OnPropertyChanged();
            }
        }

        private string _pauseButtonFill;
        public string PauseButtonFill
        {
            get => _pauseButtonFill;
            set
            {
                _pauseButtonFill = value;

                OnPropertyChanged();
            }
        }

        private string _skipToEndButtonFill;
        public string SkipToEndButtonFill
        {
            get => _skipToEndButtonFill;
            set
            {
                _skipToEndButtonFill = value;

                OnPropertyChanged();
            }
        }

        private string _selectButtonFill;
        public string SelectButtonFill
        {
            get => _selectButtonFill;
            set
            {
                _selectButtonFill = value;

                OnPropertyChanged();
            }
        }

        private string _switchOnBackground;
        public string SwitchOnBackground
        {
            get => _switchOnBackground;
            set
            {
                _switchOnBackground = value;

                OnPropertyChanged();
            }
        }

        private string _switchOffBackground;
        public string SwitchOffBackground
        {
            get => _switchOffBackground;
            set
            {
                _switchOffBackground = value;

                OnPropertyChanged();
            }
        }

        private string _switchForeground;
        public string SwitchForeground
        {
            get => _switchForeground;
            set
            {
                _switchForeground = value;

                OnPropertyChanged();
            }
        }

        private string _gainSliderMidBarFill;
        public string GainSliderMidBarFill
        {
            get => _gainSliderMidBarFill;
            set
            {
                _gainSliderMidBarFill = value;
                OnPropertyChanged();
            }
        }

        private string _gainSliderTextForeground;
        public string GainSliderTextForeground
        {
            get => _gainSliderTextForeground;
            set
            {
                _gainSliderTextForeground = value;
                OnPropertyChanged();
            }
        }

        private string _gainSliderTickForeground;
        public string GainSliderTickForeground
        {
            get => _gainSliderTickForeground;
            set
            {
                _gainSliderTickForeground = value;
                OnPropertyChanged();
            }
        }

        private string _powerButtonLightFill;
        public string PowerButtonLightFill
        {
            get => _powerButtonLightFill;
            set
            {
                _powerButtonLightFill = value;

                OnPropertyChanged();
            }
        }

        private string _powerButtonStrokeFill;
        public string PowerButtonStrokeFill
        {
            get => _powerButtonStrokeFill;
            set
            {
                _powerButtonStrokeFill = value;

                OnPropertyChanged();
            }
        }

        private string _powerButtonHighlight;
        public string PowerButtonHighlight
        {
            get => _powerButtonHighlight;
            set
            {
                _powerButtonHighlight = value;

                OnPropertyChanged();
            }
        }

        private string _monoOnFill;
        public string MonoOnFill
        {
            get => _monoOnFill;
            set
            {
                _monoOnFill = value;

                OnPropertyChanged();
            }
        }

        private string _monoOffFill;
        public string MonoOffFill
        {
            get => _monoOffFill;
            set
            {
                _monoOffFill = value;

                OnPropertyChanged();
            }
        }

        private string _stereoOnFill;
        public string StereoOnFill
        {
            get => _stereoOnFill;
            set
            {
                _stereoOnFill = value;

                OnPropertyChanged();
            }
        }

        private string _stereoOffFill;
        public string StereoOffFill
        {
            get => _stereoOffFill;
            set
            {
                _stereoOffFill = value;

                OnPropertyChanged();
            }
        }

        private string _labelForeground;
        public string LabelForeground
        {
            get => _labelForeground;
            set
            {
                _labelForeground = value;

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
        public RelayCommand SkipToEndCommand { get; set; }
        public RelayCommand OpenPlaylistCommand { get; set; }
        public RelayCommand OpenVisualisationFilterSettingsCommand { get; set; }
        public RelayCommand SetAutoplayModeToggleCommand { get; set; }
        public RelayCommand SetLoopPlayModeToggleCommand { get; set; }
        public RelayCommand SettingsCommand { get; set; }

        #endregion

        #region Initialisation

        public ReceiverViewModel()
        {
            _isInitialising = true;
            _isMono = true;

            Title = "Digital Audio Experiment(DAE)";
            SubTitle = "Mp3 Digital Audio";
            VuLabel = "DB RMS Power";
            VolumeLabel = "Volume";

            ExitCommand = new RelayCommand(() => Exit(), () => true);
            PlayCommand = new RelayCommand(async () => PlayButtonFromCommand(), () => true);
            PauseCommand = new RelayCommand(async () => await PauseButton(), () => true);
            SelectCommand = new RelayCommand(() => SelectFile(null), () => true);
            StopCommand = new RelayCommand(StopButton, () => true);
            SkipToStartCommand = new RelayCommand(SkipToStartButton, () => true);
            SkipToEndCommand = new RelayCommand(SkipToEndButton, () => true);
            OpenPlaylistCommand = new RelayCommand(OpenPlaylist, () => true);
            OpenVisualisationFilterSettingsCommand = new RelayCommand(OpenVisualisationFilterSettings, () => true);
            SetAutoplayModeToggleCommand = new RelayCommand(SetAutoplayModeToggle, () => true);
            SetLoopPlayModeToggleCommand = new RelayCommand(SetLoopPlayModeToggle, () => true);
            SettingsCommand = new RelayCommand(OpenSettings, () => true);

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
            _filterSettingsView.DataContext = FilterSettingsViewModel = new FilterSettingsViewModel();
            FilterSettingsViewModel.OnSettingsApplied += OnFilterSettingsApplied;

            var playlistViewModel = new PlaylistPageViewModel();
            playlistViewModel.DockingChangedEvent += OnDockingChanged;
            PlaylistPageViewInstance = new PlaylistPageView();
            PlaylistPageViewInstance.DataContext = playlistViewModel;
            PlaylistPageViewInstance.SetPlaylistPageComponent(new Pages.PlaylistPage());
            PlaylistPageViewInstance.SetDocked();

            PropertyChanged += OnAnyPropertyChanged;

            RaisePropertyChangedEvents();
        }

        public void LoadSettings()
        {
            var settingsData = Settings.LoadSettings(this, FilterSettingsViewModel);

            SetupColours();
            SettingsViewModel.GetSettingsInstance(this, FilterSettingsViewModel, null).ApplyCurrentTheme();

            if (string.IsNullOrEmpty(settingsData.LastPlayedFile))
            {
                return;
            }

            if (string.IsNullOrEmpty(settingsData.PlayListFile))
            {
                return;
            }

            if (!string.IsNullOrEmpty(settingsData.PlayListFile)
                && PlaylistPageViewInstance.DataContext is PlaylistPageViewModel viewModel)
            {
                viewModel.LoadPlaylistFile(settingsData.PlayListFile);

                PlaylistPageViewInstance.Owner = MainWindow.Instance;
                viewModel.IsShowing = settingsData.IsShowing;
                viewModel.IsDocked = settingsData.IsDocked;

                viewModel.Update();

                _isInitialising = false;

                if (viewModel.FileExistsInList(settingsData.LastPlayedFile))
                {
                    viewModel.SetNextSelectedToFile(settingsData.LastPlayedFile);
                    SetupWithAutoPlay(doNotAutoPlay: true);

                    return;
                }
            }

            SelectFile(settingsData.LastPlayedFile, doNotAutoPlay: true);
        }


        private void SetupColours()
        {
            // VU
            BackgroundColour = "DodgerBlue";
            NeedleColour = "Black";
            DecalColour = "Black";
            OverdriveLampColour = "Red";
            BottomCoverFill = "Black";
            OverdriveLampOffColour = "#550000";
            MeterLabelForeground = "#62e3f6";
            // Seek slider
            SliderThumbGlowOverlay = "OrangeRed";
            SliderThumbGripBarBackground = "#250a01";
            SliderThumbPointBackground = "White";
            SliderThumbBorder = "#FFACACAC";
            SliderThumbForeground = "DodgerBlue";
            SliderThumbMouseOverBackground = "#FFDCECFC";
            SliderThumbMouseOverBorder = "#FF7Eb4EA";
            SliderThumbPressedBackground = "#FFDAECFC";
            SliderThumbPressedBorder = "#FF569DE5";
            SliderThumbDisabledBackground = "#FFF0F0F0";
            SliderThumbDisabledBorder = "#FFD9D9D9";
            SliderThumbTrackBackground = "Cyan";
            SliderThumbTrackBorder = "Blue";
            // Playback controls
            SkipToStartButtonFill = "Black";
            StopButtonFill = "Black";
            PlayButtonFill = "Black";
            PauseButtonFill = "Black";
            SkipToEndButtonFill = "Black";
            SelectButtonFill = "Black";
            SwitchOnBackground = "#62e3f6";
            SwitchOffBackground = "#000040";
            SwitchForeground = "White";
            // Volume, bass, treble controls
            GainSliderMidBarFill = "Red";
            GainSliderTextForeground = "White";
            GainSliderTickForeground = "White";
            // Power button Control
            PowerButtonLightFill = "Red";
            PowerButtonStrokeFill = "Black";
            PowerButtonHighlight = "LimeGreen";
            // Stereo indicator control
            MonoOnFill = "#62e3f6";
            MonoOffFill = "#000040";
            StereoOnFill = "Red";
            StereoOffFill = "#200000";
            LabelForeground = "Black";
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
                && PlaylistPageViewInstance == null)
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
                && PlaylistPageViewInstance.DataContext is PlaylistPageViewModel newViewModel
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

            OnFilterSettingsApplied(FilterSettingsViewModel);

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

        #region Audio File Management

        public async void SelectFile(string? fileNameParameter = null, bool doNotAutoPlay = false)
        {
            if (_getFile == null
                && string.IsNullOrEmpty(fileNameParameter))
            {
                return;
            }

            var fileName = string.IsNullOrEmpty(fileNameParameter) 
                ? _getFile.Invoke(PlaylistPageViewModel.FileFiltersAudio) 
                : fileNameParameter;

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            if (_player != null
                && !_player.IsStopped())
            {
                StopButton();

                while (_player != null
                        && (!_player.IsDisposed()
                        || !_player.IsStopped()))
                {
                    Thread.Sleep(10);
                }

                _player = null;
            }

            if (PlaylistPageViewInstance != null
                && PlaylistPageViewInstance.DataContext is PlaylistPageViewModel viewModel
                && string.IsNullOrEmpty(fileNameParameter))
            {
                viewModel.RemoveAll();
            }

            OnFileSelected(fileName, doNotAutoPlay);
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

        private void OnFileSelected(string fileName, bool doNotAutoPlay)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            StopButton();

            var timeout = 30;
            var timeoutCount = 0;

            while(_player != null
                && timeoutCount < 30)
            {
                Thread.Sleep(10);
                timeoutCount++;
            }

            SetupPlayListControls(fileName);
            ResetPlayer();
            SetupWithAutoPlay(fileName: fileName, doNotAutoPlay: doNotAutoPlay);

            if (PlaylistPageViewInstance.DataContext is PlaylistPageViewModel playlistPageViewModel
                && playlistPageViewModel.PlayList.Count() == 1
                && _player == null)
            {
                ResetPlayer();
                SetupWithAutoPlay();
            }
        }

        private void SetupPlayListControls(string fileName)
        {
            if (PlaylistPageViewInstance.DataContext is PlaylistPageViewModel playlistPageViewModel)
            {
                playlistPageViewModel.Add(fileName);
            }
        }

        private void SetupWithAutoPlay(bool autoPlayOverride = false, bool fromPlayButton = false, string fileName = "", bool doNotAutoPlay = false)
        {
            if (!(PlaylistPageViewInstance.DataContext is PlaylistPageViewModel viewModel))
            {
                return;
            }

            fileName = viewModel.GetNextFile();

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

                UpdateBassTrebleSettings();
                SetTickFrequency();

                RaisePropertyChangedEvents();

                if (IsAutoPlayChecked
                    && !doNotAutoPlay
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

                if (PlaylistPageViewInstance != null
                    && viewModel != null
                    && viewModel.IsHasList)
                {
                    SetupWithAutoPlay(autoPlayOverride, fromPlayButton);
                }
            }

            DecoderType = _player?.DecoderType;
            OnPropertyChanged(nameof(DecoderType));
            Settings.SaveSettings(this);
        }

        #endregion

        #region Application Logic

        private void SetTickFrequency()
        {
            TickFrequency = SliderMaximum * _tickPercentage;
        }

        private void OpenPlaylist()
        {
            var viewModel = PlaylistPageViewInstance?.DataContext as PlaylistPageViewModel;

            if (viewModel == null)
            {
                return;
            }

            var isShowing = PlaylistPageViewInstance.GetIsViewVisible();

            if (PlaylistPageViewInstance != null
                && viewModel != null
                && (PlaylistPageViewInstance.GetIsViewVisible()
                    || (!PlaylistPageViewInstance.GetIsViewVisible() && viewModel.IsShowing)))
            {
                PlaylistPageViewInstance.Close();
                viewModel.IsShowing = false;
                viewModel.Update();

                return;
            }

            if (PlaylistPageViewInstance == null)
            {
                PlaylistPageViewInstance = new PlaylistPageView();
                PlaylistPageViewInstance.Owner = App.Current.MainWindow;
            }

            PlaylistPageViewInstance.DataContext = PlaylistPageViewInstance.DataContext == null || (PlaylistPageViewInstance.DataContext is PlaylistPageViewModel) == null
                ? new PlaylistPageViewModel()
                : PlaylistPageViewInstance.DataContext;

            PlaylistPageViewInstance.Owner = MainWindow.Instance;

            if (!viewModel.IsDocked)
            {
                PlaylistPageViewInstance.Show();
            }

            viewModel.IsShowing = true;
            viewModel.Update();
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
                _filterSettingsView.DataContext = FilterSettingsViewModel;
            }

            if (_filterSettingsView.IsLoaded)
            {
                return;
            }

            _filterSettingsView.Show();
        }

        private void OpenSettings()
        {
            var settings = new SettingsView();

            using (var viewModel = SettingsViewModel.GetSettingsInstance(this, FilterSettingsViewModel, settings.Close))
            {
                _isInitialising = true;

                settings.DataContext = viewModel;
                var result = settings.ShowDialog();

                if (!viewModel.IsReset)
                {
                    _isInitialising = false;

                    return;
                }
                
                LoadSettings();
            }
        }

        private void UpdateBassTrebleSettings()
            => _player?.SetBassTreble(_bass, _treble);

        private void Exit()
        {
            this.Dispose();
            
            Environment.Exit(0);
        }

        #endregion

        #region Callbacks

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
                && _player.IsStopped()
                && !_player.IsHardStop())
            {
                PlayInternal();

                return;
            }

            App.Current.Dispatcher.Invoke(() =>
            {
                if (IsAutoPlayChecked
                    && _player != null
                    && !_player.IsHardStop()
                    && PlaylistPageViewInstance.DataContext is PlaylistPageViewModel viewModel
                    && viewModel.IsHasList
                    && !viewModel.IsLastItem())
                {
                    ResetPlayer();
                    SetupWithAutoPlay();
                }
                else if (_player != null
                        && PlaylistPageViewInstance.DataContext is PlaylistPageViewModel playListViewModel
                        && playListViewModel.IsLastItem()
                        && _player.IsStopped())
                {
                    StopButton();
                    playListViewModel.ResetToSelectedPlayed();
                }
            });
        }

        private void OnDockingChanged(bool isDocked)
        {
            var page = PlaylistPageViewInstance.GetPlaylistPage();
            var viewModel = PlaylistPageViewInstance.DataContext as PlaylistPageViewModel;

            if (page == null)
            {
                return;
            }

            if (viewModel == null)
            {
                return;
            }

            if (viewModel.IsDocked
                && viewModel.IsShowing)
            {
                PlaylistPageViewInstance.Close();
            }
            else if (viewModel.IsShowing)
            {
                PlaylistPageViewInstance.Show();
            }

            if (_isInitialising)
            {
                return;
            }

            Settings.SaveSettings(this);
        }

        #endregion

        #region Events

        private void RaisePropertyChangedEvents()
        {
            OnPropertyChanged(nameof(IsMono)
                , nameof(HeaderData)
                , nameof(Metadata));
        }

        private void OnAnyPropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(Bass):
                case nameof(Treble):
                case nameof(IsAutoPlayChecked):
                case nameof(IsLoopPlayChecked):
                    Settings.SaveSettings(this);
                    break;
            }
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

                    PlaylistPageViewInstance?.CloseExit();
                    
                    if (PlaylistPageViewInstance?.DataContext is PlaylistPageViewModel viewModel)
                    {
                        viewModel.Dispose();
                        PlaylistPageViewInstance.DataContext = null;
                    }

                    FilterSettingsViewModel.OnSettingsApplied -= OnFilterSettingsApplied;
                    FilterSettingsViewModel?.Dispose();
                    _filterSettingsView?.Dispose();

                    PropertyChanged -= OnAnyPropertyChanged;
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
