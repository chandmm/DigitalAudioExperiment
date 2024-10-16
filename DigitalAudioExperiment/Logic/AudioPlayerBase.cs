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
using NAudio.Wave;
using System.IO;

namespace DigitalAudioExperiment.Logic
{
    public abstract class AudioPlayerBase : IAudioPlayer
    {
        #region Fields
        private readonly int _volumeScaler = 100;

        private bool _isDisposed;
        private int _bitRate;
        private Action<int> _seekPositionCallback;
        private int _volume;
        private Action _updateCallback;
        private Action _playbackStoppedCallback;
        private bool _hardStop;
        private (float, float, float) _dbRMSValues;
        private (float left, float right) _dbVuValues;

        protected bool _isPlaying;
        protected bool _isPaused;
        protected Stream _stream;
        protected bool _isSeeking; 
        protected (int, int) _duration;
        protected int _frameIndex;
        protected int _seekPosition;
        protected WaveOutEvent _waveOut;
        protected WaveStream _waveStream;
        protected string _fileName;
        protected int _rmsSampleLength = 288;

        #endregion

        #region Initialisation

        public AudioPlayerBase(string fileName)
        {
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

            PlayStream(0, 0, 0);
        }

        protected virtual void PlayStream(int sampleRate, int bits, int numberOfChannels)
        {
            if (_stream == null)
            {
                return;
            }

            if (sampleRate == 0
                && bits == 0
                && numberOfChannels == 0)
            {
                return;
            }
             
            ResourceCleanUp();

            _stream.Position = 0;
            _waveOut = new WaveOutEvent();
            _waveStream = new RawSourceWaveStream(_stream, new WaveFormat(sampleRate, bits, numberOfChannels));

            var aggregator = OutsideStreamSampleAggregatorProvider(_waveStream, _waveOut);

            if (aggregator != null)
            {
                _waveOut.DesiredLatency = 100;
                _waveOut.PlaybackStopped += PlaybackStoppedCallback;
                _waveOut.NumberOfBuffers = 4;
                _waveOut.Init(aggregator);
                _waveOut.Play();

                while (_waveOut.PlaybackState == PlaybackState.Playing
                    || _waveOut.PlaybackState == PlaybackState.Paused)
                {
                    HandlePlaybackStates(_waveOut);

                    Thread.Sleep(100);
                }

            }

            if (aggregator != null)
            {
                aggregator.RmsCalculated -= OnSampleReady;
            }
        }

        protected virtual SampleAggregator OutsideStreamSampleAggregatorProvider(WaveStream waveStream, WaveOutEvent waveOut)
        {
            ISampleProvider sampleProvider = waveStream.ToSampleProvider();

            if (sampleProvider == null)
            {
                return null;
            }

            var aggregator = new SampleAggregator(sampleProvider)
            {
                NotificationCount = _rmsSampleLength,    // Adjust as needed
                PerformRmsCalculation = true
            };

            aggregator.RmsCalculated += OnSampleReady;

            return aggregator;
        }

        protected virtual void OnSampleReady(object? sender, RmsEventArgs args)
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

            if (_isSeeking)
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

            if (waveOut.PlaybackState != PlaybackState.Paused)
            {
                _seekPositionCallback?.Invoke(_frameIndex);
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
            float MeterMax = 96.0f; // The maximum meter value

            // Map the dB values to the meter range
            float meterLeft = MapDbToMeterValue(dBLeft, dBMin, dBMax, MeterMin, MeterMax);
            float meterRight = MapDbToMeterValue(dBRight, dBMin, dBMax, MeterMin, MeterMax);

            return (meterLeft, meterRight);
        }

        public (float, float, float) GetRmsValues() => _dbRMSValues;

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

        private void PlaybackStoppedCallback(object? sender, StoppedEventArgs e)
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
