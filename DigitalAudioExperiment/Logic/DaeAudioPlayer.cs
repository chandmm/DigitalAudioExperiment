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
using SimpleMp3Decoder;
using SimpleMp3Decoder.Data;
using SimpleMp3Decoder.Logic;
using NAudio.Wave;
using System.IO;
using NAudio.Wave.SampleProviders;
using System.Linq.Expressions;

namespace DigitalAudioExperiment.Logic
{
    public class DaeAudioPlayer : IDisposable
    {
        #region Fields
        private readonly int _volumeScaler = 100;

        private DecoderSimplePcmStream? _stream;
        private bool _isDisposed;
        private string _fileName;
        private SimpleDecoder? _simpleDecoder;
        private int _bitRate;
        private int _frameIndex;
        private (int, int) _duration;
        private bool _isPlaying;
        private bool _isSeeking;
        private int _seekPosition;
        private bool _isPaused;
        private Action<int> _seekPositionCallback;
        private int _volume;
        private Action _updateCallback;
        private Action _playbackStoppedCallback;
        private bool _hardStop;
        private (float, float, float) _dbRMSValues;
        private (float left, float right) _dbVuValues;
        private WaveOutEvent _waveOut;
        private WaveStream _waveStream;

        #endregion

        #region Initialisation

        public DaeAudioPlayer(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            _fileName = fileName;

            _simpleDecoder = new SimpleDecoder(fileName, null);

            _stream = _simpleDecoder.GetStream();
            _stream.Position = 0;
            _duration = (_stream.DurationMinutes, _stream.DurationSeconds);
        }

        public void SetSeekPositionCallback(Action<int> seekPositionCallback)
            => _seekPositionCallback = seekPositionCallback;

        public void SetUpdateCallback(Action updateCallback)
            => _updateCallback = updateCallback;

        public void SetPlaybackStoppedCallback(Action playbackStopped)
            => _playbackStoppedCallback = playbackStopped;

        #endregion

        #region Logic

        public void Play()
        {
            if (_simpleDecoder == null)
            {
                return;
            }

            if (_isPlaying)
            {
                return;
            }

            InternalPlay();
        }

        private void InternalPlay()
        {
            _isPlaying = true;
            _isPaused = false;

            PlayStream();
        }

        private void PlayStream()
        {
            if (_simpleDecoder == null)
            {
                return;
            }

            using (_stream = _simpleDecoder.GetStream())
            { 
                _stream.SetBitrateCallback(UpdateInfoFromStreamCallback);

                ResourceCleanUp();

                _waveOut = new WaveOutEvent();
                _waveStream = new RawSourceWaveStream(_stream, new WaveFormat(_stream.GetSampleRate(), 16, _stream.GetNumberOfChannels()));

                
                    //var aggregator = OutsideStreamMeteringSampleProviderTesting(waveStream, waveOut); // DEBUG and TESTING only. To DELETE
                    var aggregator = OutsideStreamSampleAggregatorProviderTesting(_waveStream, _waveOut); // DEBUG and TESTING only. To DELETE

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
                    //aggregator.StreamVolume -= OnStreamVolume;
                    aggregator.RmsCalculated -= OnSampleReady;
            }
        }

        private MeteringSampleProvider OutsideStreamMeteringSampleProviderTesting(WaveStream waveStream, WaveOutEvent waveOut)
        {
            ISampleProvider sampleProvider = waveStream.ToSampleProvider();

            //var aggregator = new SampleAggregator(sampleProvider)
            //{
            //    NotificationCount = 1024,    // Adjust as needed
            //    PerformRmsCalculation = true
            //};

            var meteringProvider = new MeteringSampleProvider(sampleProvider)
            {
                SamplesPerNotification = 1152
            };

            meteringProvider.StreamVolume += OnStreamVolume;

            return meteringProvider;
        }

        private SampleAggregator OutsideStreamSampleAggregatorProviderTesting(WaveStream waveStream, WaveOutEvent waveOut)
        {
            ISampleProvider sampleProvider = waveStream.ToSampleProvider();

            if (sampleProvider == null)
            {
                return null;
            }

            var aggregator = new SampleAggregator(sampleProvider)
            {
                NotificationCount = 1152,    // Adjust as needed
                PerformRmsCalculation = true
            };

            aggregator.RmsCalculated += OnSampleReady;

            return aggregator;
        }

        private void OnSampleReady(object? sender, RmsEventArgs args)
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

            //var result = GetVuAdjustedValues(rawSampleBytes);
            //var dbValues = CalculateDBLevels(result.Item1, result.Item2, result.Item3);

            _dbVuValues = (dbValues.Item1, dbValues.Item2);
        }

        private void OnStreamVolume(object? sender, StreamVolumeEventArgs args)
        {
            var maxDbLeft = (float)(20 * Math.Log10(args.MaxSampleValues[0]));
            var maxDbRight = args.MaxSampleValues.Length > 2 
                ? (float)(20 * Math.Log10(args.MaxSampleValues[1]))
                : maxDbLeft;
            var difference = args.MaxSampleValues.Length > 2
                ? maxDbLeft - maxDbRight
                : 0;

            var dbValues = CalculateDBLevels(maxDbLeft, maxDbRight, difference);

            _dbVuValues = (dbValues.Item1,  dbValues.Item2);
        }

        public (float left, float right) GetDbVuValues()
            => _dbVuValues;

        private void HandlePlaybackStates (WaveOutEvent waveOut)
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

        public void Seek(int seekPosition)
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

        public (double left, double right) GetVUMeterValues()
        {
            var level = _stream.GetRmsValues();
            return CalculateDBLevels(level.Item1, level.Item2, level.Item3);
        }

        private float MapDbToMeterValue(float dBValue, float dBMin, float dBMax, float MeterMin, float MeterMax)
        {
            // Ensure the dBValue stays within the expected range
            dBValue = Math.Max(dBMin, Math.Min(dBMax, dBValue));

            // Linear mapping from dB scale to VU meter scale
            float MeterValue = ((dBValue - dBMin) / (dBMax - dBMin)) * (MeterMax - MeterMin) + MeterMin;

            return MeterValue;
        }

        private (float, float) CalculateDBLevels(float dBLeft, float dBRight, float difference)
        {

            // Define your dB range and VU meter range
            float dBMin = -60.0f;  // The minimum dB value
            float dBMax = 0.0f;    // The maximum dB value
            float MeterMin = 0.0f; // The minimum meter value
            float MeterMax = 96.0f; // The maximum meter value

            // Map the dB values to the meter range
            float meterLeft = MapDbToMeterValue(dBLeft, dBMin, dBMax, MeterMin, MeterMax);
            float meterRight = MapDbToMeterValue(dBRight, dBMin, dBMax, MeterMin, MeterMax);

            return (meterLeft, meterRight);
        }

        public bool IsStopped
            => !_isPlaying;

        #endregion

        #region DB RMS value calculations

        public (float, float, float) GetRmsValues() => _dbRMSValues;

        #endregion

        #region Fetch File Information Logic

        public SimpleDecoder? GetDecoder()
            => _simpleDecoder;

        public string GetAudioFileInfo()
        {
            if (_simpleDecoder == null)
            {
                throw new NullReferenceException("Decoder is not initialised.");
            }

            if (_simpleDecoder.GetFrameCount() == 0)
            {
                throw new ApplicationException("No frames present or invalid audio file format.");
            }

            return _simpleDecoder.GetFrames().First().ToStringShort();
        }

        public int GetBitRate()
            => _stream.GetBitRate();

        public int GetBitratePerFrame()
            => _bitRate;

        public bool GetIsMonoChannel()
            => HeaderInfoUtils.GetNumberOfChannels(_simpleDecoder?.GetFrames().First().Header)  < 2;

        public (int, int) Duration()
            => _duration;

        public double GetElapsed()
        {
            if (_stream == null
                || _simpleDecoder == null)
            {
                return 0;
            }

            return _stream.CalculateDuration(_simpleDecoder.GetFrames().GetRange(0, _frameIndex));
        }

        public int? GetFrameCount()
            => _simpleDecoder?.GetFrameCount();

        public void SetHardStop(bool hardStop)
            => _hardStop = hardStop;

        public bool IsHardStop
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

        private void UpdateInfoFromStreamCallback(int bitRate, int frameIndex)
        {
            _bitRate = bitRate;
            _frameIndex = frameIndex;
        }

        public string GetMetadata()
            => string.Join("\r\n",_simpleDecoder?.GetMetadata());

        #endregion

        #region Cleanup and Dispose

        // Disposing this way as unsafe thread operation
        // may cause attempts to read disposed stream after 'using' block exits.
        private void ResourceCleanUp()
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

                    _simpleDecoder?.Dispose();
                    _simpleDecoder = null;
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
