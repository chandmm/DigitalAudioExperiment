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

namespace DigitalAudioExperiment.Logic
{
    public class AudioPlayerFlac : AudioPlayerBase, IAudioPlayer
    {
        private bool _isDisposed;
        private AudioFileReader? _reader;

        public AudioPlayerFlac(string fileName) 
            : base(fileName)
        {
        }
        
        public override void Initialise()
        {
            _reader = new AudioFileReader(_fileName);
            _duration = ((int)_reader.TotalTime.Minutes, (int)(_reader.TotalTime.TotalSeconds % 60));
        }

        protected override void PlayStream(WaveFormat? waveFormatNotUsed)
        {
            _reader = new AudioFileReader(_fileName);
            _reader.Position = 0;
            _stream = _reader;

            base.PlayStream(_reader.WaveFormat);
        }

        public override string GetAudioFileInfo()
        {
            return $"Install TagLibSharp to fetch file info.";
        }

        public override double GetElapsed()
            => _reader.CurrentTime.TotalSeconds;

        public override int? GetFrameCount()
            => (int)_reader.Length;

        public override bool GetIsMonoChannel()
            => _reader.WaveFormat.Channels == 1;

        public override string GetMetadata()
        {
            return $"Install TagLibSharp to fetch metadata.";
        }

        #region Cleanup and Dispose

        private void Dispose(bool _isDisposing)
        {
            if (!_isDisposed)
            {
                if (_isDisposing)
                {
                    if (_reader != null)
                    {
                        _reader.Dispose();
                        _reader = null;
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
