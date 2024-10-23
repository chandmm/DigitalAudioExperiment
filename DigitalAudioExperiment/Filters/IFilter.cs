using NAudio.Dsp;
using NAudio.Wave;

namespace DigitalAudioExperiment.Filters
{
    public interface IFilter
    {
        void CalculateRms(int samplesRead, float[] buffer, int offset, int notificationCount);
        float Transform(float sample, int channel);
        void UpdateFilterSettings(float lowepassCutoffFrequency,  float highepassCutoffFrequency, int filterOrder);
        FilterType GetFilterType();
    }
}
