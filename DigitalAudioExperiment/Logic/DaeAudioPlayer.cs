using Mp3DecoderSimple;
using NAudio.Wave;

namespace DigitalAudioExperiment.Logic
{
    public class DaeAudioPlayer : IDisposable
    {
        #region Fields

        private bool _isDisposed;
        private string _fileName;
        private SimpleDecoder? _simpleDecoder;

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
            using (var simpleStream = _simpleDecoder.GetStream())
            {

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
