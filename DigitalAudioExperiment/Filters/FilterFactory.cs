using NAudio.Wave;

namespace DigitalAudioExperiment.Filters
{
    public enum FilterType
    {
        LowPass,
        HighPass,
        BandPass,
        ButterworthBandpass
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
                    return new FilterBandpass(waveFormat, lowPassCutoff, highPassCutoff);
                case FilterType.ButterworthBandpass:
                    return new FilterButterworthBandpass(waveFormat, lowPassCutoff, highPassCutoff);
            }

            throw new NotImplementedException();
        }
    }
}
