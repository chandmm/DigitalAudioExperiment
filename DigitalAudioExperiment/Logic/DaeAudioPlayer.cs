using Mp3DecoderSimple;
using NAudio.Wave;
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

        #endregion

        #region Initialisation

        public DaeAudioPlayer(string fileName)
        {
            _fileName = fileName;

            _simpleDecoder = new SimpleDecoder(fileName, null);
        }

        #endregion

        #region Logic

        public void Play()
        {
            if (_simpleDecoder == null)
            {
                return;
            }
        }

        private void InternalPlay()
        {
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

                        while (waveOut.PlaybackState == PlaybackState.Playing)
                        {
                            Thread.Sleep(40);
                        }
                    }
                }
            }
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

        #endregion

        #region Callback Methods

        private void PlaybackStoppedCallback(object? sender, StoppedEventArgs e)
        {
            throw new NotImplementedException();
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
