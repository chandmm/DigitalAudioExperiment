using NAudio.Dsp;
using NAudio.Wave;

namespace DigitalAudioExperiment.Filters
{
    public class FilterHighpass : FilterAbstractBase, IFilter
    {
        private BiQuadFilter[] _filters;
        private WaveFormat _waveFormat;

        public FilterHighpass(WaveFormat waveFormat, float highpassCutoffFrequency)
        {
            _waveFormat = waveFormat;
            _filters = new BiQuadFilter[_waveFormat.Channels];

            CreateFilter(_waveFormat, highpassCutoffFrequency);
        }
        public override void CalculateRms(int samplesRead, float[] buffer, int offset, int filterOrder)
        {
            throw new NotImplementedException();
        }

        public override float Transform(float sample, int channel)
        {
            throw new NotImplementedException();
        }

        protected override BiQuadFilter CreateFilter(WaveFormat waveFormat, float highpassCutoffFrequency)
        {
            for (int channel = 0; channel < waveFormat.Channels; channel++)
            {
                _filters[channel] = BiQuadFilter.HighPassFilter(waveFormat.SampleRate, highpassCutoffFrequency, 1.0f);
            }

            return null;
        }

        protected override void CreateFilter(WaveFormat waveFormat, float lowPassCutoff, float highPassCutoff, int filterOrder)
        {
            throw new NotImplementedException();
        }

        public override FilterType GetFilterType()
            => FilterType.Highpass;

        public override void UpdateFilterSettings(float lowepassCutoffFrequency, float highepassCutoffFrequency, int filterOrder)
            => CreateFilter(_waveFormat, highepassCutoffFrequency);
    }
}
