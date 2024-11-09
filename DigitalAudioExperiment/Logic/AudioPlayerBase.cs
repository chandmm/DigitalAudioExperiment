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
using DigitalAudioExperiment.Events;
using DigitalAudioExperiment.Filters;
using DigitalAudioExperiment.ViewModel;
using NAudio.Wave;
using System.IO;

namespace DigitalAudioExperiment.Logic
{
    public abstract class AudioPlayerBase : IAudioPlayer
    {
        #region Statics and Constants

        public SynchronizationContext? Context { get; set; }

        #endregion

        #region Fields
        private readonly int _volumeScaler = 100;
        private static object _lock = new object();

        private bool _isDisposed;
        private int _volume;
        private Action _updateCallback;
        private Action _playbackStoppedCallback;
        private bool _hardStop;
        private (float, float, float) _dbRMSValues;
        private (float left, float right) _dbVuValues;
        private ISampleProvider _sampleAggregator;

        protected int _bitRate;
        protected Action<int> _seekPositionCallback;
        protected bool _isPlaying;
        protected bool _isPaused;
        protected Stream _stream;
        protected bool _isSeeking; 
        protected (int, int) _duration;
        protected int? _frameIndex;
        protected int _seekPosition;
        protected WaveOutEvent _waveOut;
        protected WaveStream _waveStream;
        protected string _fileName;
        protected int _rmsSampleLength = 288;
        protected FilterSettingsViewModel _filterSettingsViewModel;

        public string DecoderType { get; protected set; }

        #endregion

        #region Initialisation

        public AudioPlayerBase(string fileName)
        {
            DecoderType = "No known decoder";
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            _fileName = fileName;
        }

        public abstract void Initialise();

        public virtual void SetSeekPositionCallback(Action<int> seekPositionCallback)
            => _seekPositionCallback = seekPositionCallback;

        public virtual void SetUpdateCallback(Action updateCallback)
            => _updateCallback = updateCallback;

        public virtual void SetPlaybackStoppedCallback(Action playbackStopped)
            => _playbackStoppedCallback = playbackStopped;

        #endregion

        #region Logic

        public virtual void Play()
        {
            if (_isPlaying)
            {
                return;
            }

            InternalPlay();
        }

        protected virtual void InternalPlay()
        {
            _isPlaying = true;
            _isPaused = false;

            PlayStream(null);
        }

        protected virtual void PlayStream(WaveFormat? waveFormat)
        {
            if (_stream == null)
            {
                return;
            }

            if (waveFormat == null)
            {
                return;
            }
             
            ResourceCleanUp();

            _stream.Position = 0;
            _waveOut = new WaveOutEvent();
            _waveStream = new RawSourceWaveStream(_stream, waveFormat);

            _sampleAggregator = OutsideStreamSampleAggregatorProvider(_waveStream, _waveOut);

            if (_sampleAggregator != null)
            {
                _waveOut.DesiredLatency = 100;
                _waveOut.PlaybackStopped += PlaybackStoppedCallback;
                _waveOut.NumberOfBuffers = 4;
                _waveOut.Init(_sampleAggregator);
                _waveOut.Play();

                while (_waveOut.PlaybackState == PlaybackState.Playing
                    || _waveOut.PlaybackState == PlaybackState.Paused)
                {
                    try
                    {
                        HandlePlaybackStates(_waveOut);
                    }
                    catch { };

                    Thread.Sleep(100);
                }

            }

            if (_sampleAggregator != null)
            {
                (_sampleAggregator as SampleAggregator).RmsCalculated -= OnSampleReady;

                _sampleAggregator = null;
            }
        }

        protected virtual SampleAggregator OutsideStreamSampleAggregatorProvider(WaveStream waveStream, WaveOutEvent waveOut)
        {
            ISampleProvider sampleProvider = waveStream.ToSampleProvider();

            if (sampleProvider == null)
            {
                return null;
            }

            var aggregator = new SampleAggregator(sampleProvider
                , FilterFactory.GetFilterInterface(_filterSettingsViewModel.FilterTypeSet.FilterTypeValue, 
                waveStream.WaveFormat, 
                _filterSettingsViewModel.CutoffFrequency - (_filterSettingsViewModel.Bandwidth/2),
                _filterSettingsViewModel.CutoffFrequency + (_filterSettingsViewModel.Bandwidth/2), 
                _filterSettingsViewModel.FilterOrder), _filterSettingsViewModel.IsFilterOutput)
            {
                NotificationCount = _rmsSampleLength,
                PerformRmsCalculation = true
            };

            aggregator.RmsCalculated += OnSampleReady;

            return aggregator;
        }

        protected virtual void OnSampleReady(object? sender, RmsEventArgs args)
        {
            lock (_lock)
            {
                var rmsSamples = args.RmsValues;

                var maxDbLeft = (float)(20 * Math.Log10(rmsSamples[0]));
                var maxDbRight = rmsSamples.Length > 1
                    ? (float)(20 * Math.Log10(rmsSamples[1]))
                    : maxDbLeft;
                var difference = rmsSamples.Length > 1
                    ? maxDbLeft - maxDbRight
                    : 0;

                var dbValues = CalculateDBLevels(maxDbLeft, maxDbRight, difference);

                _dbVuValues = (dbValues.Item1, dbValues.Item2);
            }
        }

        public (float left, float right) GetDbVuValues()
            => _dbVuValues;

        protected virtual void HandlePlaybackStates (WaveOutEvent waveOut)
        {
            if (!_isPlaying)
            {
                waveOut.Stop();

                return;
            }

            if (_isPaused
                && waveOut.PlaybackState != PlaybackState.Paused)
            {
                waveOut.Pause();

                return;
            }

            if (_isSeeking
                && _frameIndex != null)
            {
                _isSeeking = false;
                _isPaused = false;
                _stream?.Seek(_seekPosition, SeekOrigin.Begin);
            }

            if (waveOut.PlaybackState == PlaybackState.Paused
                && !_isPaused)
            {
                waveOut.Play();
            }

            if (waveOut.PlaybackState != PlaybackState.Paused
                && _frameIndex != null)
            {
                _seekPositionCallback?.Invoke(_frameIndex ?? 0);
            }

            if ((waveOut.Volume * _volumeScaler) != _volume)
            {
                waveOut.Volume = (float)_volume / _volumeScaler;
            }

            _updateCallback?.Invoke();
        }

        public void Stop()
        {
            _isPlaying = false;
        }

        public virtual void Seek(int seekPosition)
        {
            _seekPosition = seekPosition;
            _isSeeking = true;
        }

        public void Pause()
        {
            if (!_isPlaying)
            {
                return; 
            }

            _isPaused = !_isPaused;
        }

        public void SetVolume(int volume)
            => _volume = volume;


        public bool IsStopped()
            => !_isPlaying;

        public void SetContext(SynchronizationContext context)
            => Context = context;

        #endregion

        #region DB RMS value calculations

        private float MapDbToMeterValue(float dBValue, float dBMin, float dBMax, float MeterMin, float MeterMax)
        {
            // Ensure the dBValue stays within the expected range
            dBValue = Math.Max(dBMin, Math.Min(dBMax, dBValue));

            // Linear mapping from dB scale to VU meter scale
            float MeterValue = ((dBValue - dBMin) / (dBMax - dBMin)) * (MeterMax - MeterMin) + MeterMin;

            return MeterValue;
        }

        protected virtual (float, float) CalculateDBLevels(float dBLeft, float dBRight, float difference)
        {

            // Define dB range and VU meter range
            float dBMin = -60.0f;  // The minimum dB value
            float dBMax = 0.0f;    // The maximum dB value
            float MeterMin = 0.0f; // The minimum meter value
            float MeterMax = 100.0f; // The maximum meter value

            // Map the dB values to the meter range
            float meterLeft = MapDbToMeterValue(dBLeft, dBMin, dBMax, MeterMin, MeterMax);
            float meterRight = MapDbToMeterValue(dBRight, dBMin, dBMax, MeterMin, MeterMax);

            return (meterLeft, meterRight);
        }

        public (float, float, float) GetRmsValues() => _dbRMSValues;

        public void UpdateFilterSettings(FilterSettingsViewModel filterSettingsViewModel)
        {
            _filterSettingsViewModel = filterSettingsViewModel;

            if ( _sampleAggregator == null)
            {
                return;
            }

            (_sampleAggregator as SampleAggregator).UpdateFilterSettings(_filterSettingsViewModel.CutoffFrequency, _filterSettingsViewModel.Bandwidth, _filterSettingsViewModel.FilterOrder, _filterSettingsViewModel.IsFilterOutput);
            // TODO Filter change code.
        }

        #endregion

        #region Fetch File Information Logic

        public abstract string GetAudioFileInfo();

        public virtual int GetBitRate()
            => _bitRate;

        public virtual int GetBitratePerFrame()
            => _bitRate;

        public abstract bool GetIsMonoChannel();

        public virtual (int, int) Duration()
            => _duration;

        public abstract double GetElapsed();

        public abstract int? GetFrameCount();

        public virtual void SetHardStop(bool hardStop)
            => _hardStop = hardStop;

        public virtual bool IsHardStop()
            => _hardStop;

        #endregion

        #region Callback Methods

        protected virtual void PlaybackStoppedCallback(object? sender, StoppedEventArgs e)
        {
            _isPlaying = false;
            ResourceCleanUp();
            _updateCallback?.Invoke();
            _playbackStoppedCallback?.Invoke();
        }

        protected virtual void UpdateInfoFromStreamCallback(int bitRate, int frameIndex)
        {
            _bitRate = bitRate;
            _frameIndex = frameIndex;
        }

        public abstract string GetMetadata();

        public bool IsDisposed()
            => _isDisposed;

        #endregion

        #region Cleanup and Dispose

        // Disposing this way instea of 'using' block as unsafe thread operation
        // may cause attempts to read disposed stream after 'using' block exits.
        protected virtual void ResourceCleanUp()
        {
            try
            {
                if (_waveOut != null)
                {
                    _waveOut.Dispose();
                }

                if (_waveStream != null)
                {
                    _waveStream.Dispose();
                }
            }
            catch(Exception exception)
            {
                _ = exception.Message;

                _waveOut = null;
                _waveStream = null;
            }

            if ((_sampleAggregator as SampleAggregator) != null)
            {
                (_sampleAggregator as SampleAggregator).RmsCalculated -= OnSampleReady;
                (_sampleAggregator as SampleAggregator)?.Dispose();
            }
        }

        private void Dispose(bool _isDisposing)
        {
            if (!_isDisposed)
            {
                if (_isDisposing)
                {
                    if (_isPlaying)
                    {
                        _isPlaying = false;
                    }

                    ResourceCleanUp();

                    if (_stream != null)
                    {
                        _stream?.Dispose();
                        _stream = null;
                    }

                    Context = null;
                }

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion
    }
}
