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
using System.Text;
using System.Windows;

namespace DigitalAudioExperiment.Logic
{
    public class AudioPlayerFlac : AudioPlayerBase, IAudioPlayer
    {
        private bool _isDisposed;
        private AudioFileReader? _reader;
        private StringBuilder _metaData = new StringBuilder();
        private StringBuilder _fileInfo = new StringBuilder();

        public AudioPlayerFlac(string fileName) 
            : base(fileName)
        {
            DecoderType = "NAudio Flac.";
        }
        
        public override void Initialise()
        {
            try
            {
                FetchFileMetadata();
                _reader = new AudioFileReader(_fileName);
                _duration = ((int)_reader.TotalTime.Minutes, (int)(_reader.TotalTime.TotalSeconds % 60));
            }
            catch(Exception exception)
            {
                _reader?.Dispose();
                _reader = null;

                MessageBox.Show(exception.Message);
            }
        }

        private void FetchFileMetadata()
        {
            using (var tagLibFile = TagLib.File.Create(_fileName))
            {
                var metaData = tagLibFile.GetTag(TagLib.TagTypes.FlacMetadata, true);

                _metaData.AppendLine($"Title: {metaData.Title}");
                _metaData.AppendLine($"Genres: {metaData.JoinedGenres}");
                _metaData.AppendLine($"Artists: {metaData.JoinedAlbumArtists}");
                _metaData.AppendLine($"Performers: {metaData.JoinedPerformers}");
                _metaData.AppendLine($"Description: {metaData.Description}");

                _bitRate = tagLibFile.Properties.AudioBitrate;

                _fileInfo.AppendLine($"Sample rate: {tagLibFile.Properties.AudioSampleRate}");
                _fileInfo.AppendLine($"Bits per sample: {tagLibFile.Properties.BitsPerSample}");
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
            => _fileInfo.ToString();

        public override double GetElapsed()
            => _reader == null ? 0 : _reader.CurrentTime.TotalSeconds;

        public override int? GetFrameCount()
            => (int?)_reader?.Length;

        public override bool GetIsMonoChannel()
            => _reader?.WaveFormat.Channels == 1;

        public override string GetMetadata()
            => _metaData.ToString();

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
