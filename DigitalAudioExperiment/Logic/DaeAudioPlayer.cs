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
using Mp3DecoderSimple;
using Mp3DecoderSimple.Data;
using Mp3DecoderSimple.Logic;
using NAudio.Wave;
using System.IO;

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

        #endregion

        #region Initialisation

        public DaeAudioPlayer(string fileName)
        {
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

                using (WaveOutEvent waveOut = new WaveOutEvent())
                {
                    using (WaveStream waveStream = new RawSourceWaveStream(_stream, new WaveFormat(_stream.GetSampleRate(), 16, _stream.GetNumberOfChannels())))
                    {
                        waveOut.DesiredLatency = 100;
                        waveOut.PlaybackStopped += PlaybackStoppedCallback;
                        waveOut.Init(waveStream);
                        waveOut.Play();

                        while (waveOut.PlaybackState == PlaybackState.Playing
                            || waveOut.PlaybackState == PlaybackState.Paused)
                        {
                            HandlePlaybackStates(waveOut);

                            Thread.Sleep(40);
                        }
                    }
                }
            }
        }

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
            _isSeeking = true;
            _seekPosition = seekPosition;
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
            return _stream.CalculateDuration(_simpleDecoder.GetFrames().GetRange(0, _frameIndex));
        }

        public int? GetFrameCount()
            => _simpleDecoder?.GetFrameCount();

        #endregion

        #region Callback Methods

        private void PlaybackStoppedCallback(object? sender, StoppedEventArgs e)
        {
            //throw new NotImplementedException();
            _isPlaying = false;
        }

        private void UpdateInfoFromStreamCallback(int bitRate, int frameIndex)
        {
            _bitRate = bitRate;
            _frameIndex = frameIndex;
        }

        #endregion

        #region Dispose

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

                    _stream?.Dispose();
                    _stream = null;
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
