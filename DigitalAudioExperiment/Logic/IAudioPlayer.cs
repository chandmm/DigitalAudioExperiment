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
namespace DigitalAudioExperiment.Logic
{
    public interface IAudioPlayer: IDisposable
    {
        public void Initialise();

        #region Playback control
        public void SetSeekPositionCallback(Action<int> seekPositionCallback);

        public void SetUpdateCallback(Action updateCallback);

        public void SetPlaybackStoppedCallback(Action playbackStopped);

        #endregion

        #region Logic

        public void Play();

        public (float left, float right) GetDbVuValues();

        public void Stop();

        public void Seek(int seekPosition);

        public void Pause();

        public void SetVolume(int volume);

        public bool IsStopped();

        #endregion

        #region DB RMS value calculations

        public (float, float, float) GetRmsValues();

        #endregion

        #region Fetch File Information Logic

        public string GetAudioFileInfo();

        public int GetBitRate();

        public int GetBitratePerFrame();

        public bool GetIsMonoChannel();
        public (int, int) Duration();

        public double GetElapsed();

        public int? GetFrameCount();

        public void SetHardStop(bool hardStop);

        public bool IsHardStop();

        public string GetMetadata();

        #endregion

        #region Cleanup and Dispose

        void Dispose();

        #endregion
    }
}
