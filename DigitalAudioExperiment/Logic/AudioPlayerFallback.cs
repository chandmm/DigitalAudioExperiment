using NAudio.Wave;
using System.IO;
using System.Text;
using System.Windows;

namespace DigitalAudioExperiment.Logic
{
    public class AudioPlayerFallback : AudioPlayerBase, IAudioPlayer
    {
        private bool _isDisposed;
        private AudioFileReader? _reader;
        private StringBuilder _metaData = new StringBuilder();
        private StringBuilder _fileInfo = new StringBuilder();

        public AudioPlayerFallback(string fileName) 
            : base(fileName)
        {
            DecoderType = "Fallback Mode.";
        }

        public override void Initialise()
        {
            try
            {
                _reader = new AudioFileReader(_fileName);
                _duration = ((int)_reader.TotalTime.Minutes, (int)(_reader.TotalTime.TotalSeconds % 60));
            }
            catch (Exception exception)
            {
                _reader?.Dispose();
                _reader = null;

                MessageBox.Show(exception.Message);

                _isPlaying = false;
                Dispose();
            }
        }

        protected override void PlayStream(WaveFormat? waveFormatNotUsed)
        {
            if (_reader == null)
            {
                return;
            }

            _reader = new AudioFileReader(_fileName);
            _reader.Position = 0;
            _stream = _reader;

            base.PlayStream(_reader.WaveFormat);
        }

        protected override void HandlePlaybackStates(WaveOutEvent waveOut)
        {
            base.HandlePlaybackStates(waveOut);

            if (_isSeeking)
            {
                _isSeeking = false;
                _isPaused = false;
                _stream?.Seek(_seekPosition, SeekOrigin.Begin);
            }

            if (waveOut.PlaybackState != PlaybackState.Paused
                && waveOut.PlaybackState != PlaybackState.Stopped)
            {
                _seekPositionCallback?.Invoke((int)_reader.Position);
            }
        }

        public override string GetAudioFileInfo()
            => $"Playing: {Path.GetFileName(_fileName)}";

        public override double GetElapsed()
            => _reader == null ? 0 : _reader.CurrentTime.TotalSeconds;

        public override int? GetFrameCount()
            => (int?)_reader?.Length;

        public override bool GetIsMonoChannel()
            => _reader?.WaveFormat.Channels == 1;

        public override string GetMetadata()
            => "This is a fallback player. No data available.";
    }
}
