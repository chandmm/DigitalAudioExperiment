

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
