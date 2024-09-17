using Mp3DecoderSimple;
using Mp3DecoderSimple.Data;
using NAudio.Wave;
using System.IO;
using System.Windows.Input;

namespace DigitalAudioExperiment.Logic
{
    public class DaeAudioPlayer : IDisposable
    {
        #region Fields

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

        #endregion

        #region Initialisation

        public DaeAudioPlayer(string fileName)
        {
            _fileName = fileName;

            _simpleDecoder = new SimpleDecoder(fileName, null);
        }

        public void SetSeekPositionCallback(Action<int> seekPositionCallback)
            => _seekPositionCallback = seekPositionCallback;

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
            using (var simpleStream = _simpleDecoder.GetStream())
            {
                simpleStream.Position = 0;
                simpleStream.SetBitrateCallback(UpdateInfoFromStreamCallback);
                _duration = (simpleStream.DurationMinutes, simpleStream.DurationSeconds);

                using (WaveOutEvent waveOut = new WaveOutEvent())
                {
                    using (WaveStream waveStream = new RawSourceWaveStream(simpleStream, new WaveFormat(simpleStream.GetSampleRate(), 16, simpleStream.GetNumberOfChannels())))
                    {
                        waveOut.DesiredLatency = 150;
                        waveOut.PlaybackStopped += PlaybackStoppedCallback;
                        waveOut.Init(waveStream);
                        waveOut.Play();

                        while (waveOut.PlaybackState == PlaybackState.Playing
                            || waveOut.PlaybackState == PlaybackState.Paused)
                        {
                            HandlePlaybackStates(simpleStream, waveOut);

                            Thread.Sleep(40);
                        }
                    }
                }
            }
        }

        private void HandlePlaybackStates (DecoderSimplePcmStream simpleStream, WaveOutEvent waveOut)
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
                simpleStream.Seek(_seekPosition, SeekOrigin.Begin);
            }

            if (waveOut.PlaybackState == PlaybackState.Paused
                && !_isPaused)
            {
                waveOut.Play();
            }

            _seekPositionCallback?.Invoke(_frameIndex);
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

        #endregion

        #region Fetch Playback Information Logic

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

            return _simpleDecoder.GetFrames().First().ToString();
        }

        public (int, int) Duration()
            => _duration;

        public int GetFrameCount()
            => (int)_simpleDecoder?.GetFrameCount();

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
