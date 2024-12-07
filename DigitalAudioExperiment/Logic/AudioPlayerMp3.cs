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
using SimpleMp3Decoder;
using SimpleMp3Decoder.Data;
using SimpleMp3Decoder.Logic;

namespace DigitalAudioExperiment.Logic
{
    public class AudioPlayerMp3 : AudioPlayerBase, IAudioPlayer
    {
        #region Fields
        private SimpleDecoder? _simpleDecoder;
        private DecoderSimplePcmStream? _decoderPcmStream;
        private bool _isDisposed;
        #endregion

        #region Initialisation

        public AudioPlayerMp3(string fileName)
            : base(fileName)
        {
            DecoderType = "Simple Decoder by Michael Chand";
        }

        public override void Initialise()
        {
            _simpleDecoder = new SimpleDecoder(_fileName, null);

            _decoderPcmStream = _simpleDecoder.GetStream();
            _decoderPcmStream.Position = 0;
            _duration = (_decoderPcmStream.DurationMinutes, _decoderPcmStream.DurationSeconds);
        }

        #endregion

        #region Logic

        public override void Play()
        {
            if (_simpleDecoder == null)
            {
                return;
            }

            base.Play();
        }

        protected override void PlayStream(WaveFormat waveFormatNotUsed)
        {
            if (_simpleDecoder == null)
            {
                return;
            }

            using (_decoderPcmStream = _simpleDecoder.GetStream())
            {
                _decoderPcmStream.SetBitrateCallback(UpdateInfoFromStreamCallback);

                _stream = _decoderPcmStream;

                base.PlayStream(new WaveFormat(_decoderPcmStream.GetSampleRate(), 16, _decoderPcmStream.GetNumberOfChannels()));
            }
        }

        #endregion

        #region Fetch File Information Logic

        public override string GetAudioFileInfo()
        {
            if (_simpleDecoder == null)
            {
                throw new NullReferenceException("Decoder is not initialised.");
            }

            if (_simpleDecoder.GetFrameCount() == 0)
            {
                throw new ApplicationException("No frames present or invalid audio file format.");
            }

            return string.Join("\r\n", _simpleDecoder.GetFrames().First().ToStringShort().Split("\r\n").Take(4));
        }

        public override bool GetIsMonoChannel()
            => HeaderInfoUtils.GetNumberOfChannels(_simpleDecoder?.GetFrames().First().Header)  < 2;

        public override double GetElapsed()
        {
            if (_decoderPcmStream == null
                || _simpleDecoder == null)
            {
                return 0;
            }

            return _decoderPcmStream.CalculateDuration(_simpleDecoder.GetFrames().GetRange(0, _frameIndex ?? 0));
        }

        public override int? GetFrameCount()
            => _simpleDecoder?.GetFrameCount();

        public override string GetMetadata()
            => string.Join("\r\n", _simpleDecoder?.GetMetadata());

        #endregion

        #region Cleanup and Dispose

        private void Dispose(bool _isDisposing)
        {
            if (!_isDisposed)
            {
                if (_isDisposing)
                {
                    // Dispose of any custom decoder.
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
