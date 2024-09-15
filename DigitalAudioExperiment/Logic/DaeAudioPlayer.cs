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

            using (var simpleStream = _simpleDecoder.GetStream())
            {

            }
        }

        private void InternalPlay()
        {
            
        }

        #endregion

        #region Fetch Playback Information Logic

        public SimpleDecoder? GetDecoder()
            => _simpleDecoder;

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
