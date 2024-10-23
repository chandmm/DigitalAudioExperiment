using NAudio.Wave;

namespace DigitalAudioExperiment.Filters
{
    public enum FilterType
    {
        Lowpass,
        Highpass,
        Bandpass,
        ButterworthBandpass
    }

    public static class FilterFactory
    {
        public static IFilter GetFilterInterface(FilterType filterType, WaveFormat waveFormat, float lowpassCutoffFrequency, float highpassCutoffFrequency, int filterORder)
        {
            switch (filterType)
            {
                case FilterType.Lowpass:
                    return new FilterLowpass(waveFormat, lowpassCutoffFrequency);
                case FilterType.Highpass:
                    return new FilterHighpass(waveFormat, highpassCutoffFrequency);
                case FilterType.Bandpass:
                    return new FilterBandpass(waveFormat, lowpassCutoffFrequency, highpassCutoffFrequency);
                case FilterType.ButterworthBandpass:
                    return new FilterButterworthBandpass(waveFormat, lowpassCutoffFrequency, highpassCutoffFrequency);
            }

            throw new NotImplementedException();
        }
    }
}
