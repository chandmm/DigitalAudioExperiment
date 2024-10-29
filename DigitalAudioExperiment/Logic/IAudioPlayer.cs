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
using DigitalAudioExperiment.ViewModel;

namespace DigitalAudioExperiment.Logic
{
    public interface IAudioPlayer: IDisposable
    {
        void Initialise();

        #region Playback control
        void SetSeekPositionCallback(Action<int> seekPositionCallback);

        void SetUpdateCallback(Action updateCallback);

        void SetPlaybackStoppedCallback(Action playbackStopped);

        #endregion

        #region Logic

        void Play();

        (float left, float right) GetDbVuValues();

        void Stop();

        void Seek(int seekPosition);

        void Pause();

        void SetVolume(int volume);

        bool IsStopped();

        #endregion

        #region DB RMS value calculations

        (float, float, float) GetRmsValues();
        void UpdateFilterSettings(FilterSettingsViewModel filterSettingsViewModel);

        #endregion

        #region Fetch File Information Logic

        string GetAudioFileInfo();

        int GetBitRate();

        int GetBitratePerFrame();

        bool GetIsMonoChannel();
        (int, int) Duration();

        double GetElapsed();

        int? GetFrameCount();

        void SetHardStop(bool hardStop);

        bool IsHardStop();

        string GetMetadata();

        bool IsDisposed();

        string DecoderType { get; }

        #endregion

        #region Cleanup and Dispose

        void Dispose();

        #endregion
    }
}
