using NAudio.Wave;

namespace DigitalAudioExperiment.Filters
{
    public enum FilterType
    {
        LowPass,
        HighPass,
        BandPass,
        Butterworth
    }

    public static class FilterFactory
    {
        public static IFilter GetFilterInterface(FilterType filterType, WaveFormat waveFormat, float lowPassCutoff, float highPassCutoff, int filterORder)
        {
            switch (filterType)
            {
                case FilterType.LowPass:
                    throw new NotImplementedException();
                case FilterType.HighPass:
                    throw new NotImplementedException();
                case FilterType.BandPass:
                    return new FilterButterworthBandpass(waveFormat, lowPassCutoff, highPassCutoff);
                case FilterType.Butterworth:
                    throw new NotImplementedException();
            }

            throw new NotImplementedException();
        }
    }
}
