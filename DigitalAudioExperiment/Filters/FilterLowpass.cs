using NAudio.Dsp;
using NAudio.Wave;
using System.Runtime.CompilerServices;

namespace DigitalAudioExperiment.Filters
{
    public class FilterLowpass : FilterAbstractBase, IFilter
    {
        private BiQuadFilter[] _filters;
        private WaveFormat _waveFormat;

        public FilterLowpass(WaveFormat waveFormat, float lowpassCutoffFrequency)
        {
            _waveFormat = waveFormat;
            _filters = new BiQuadFilter[_waveFormat.Channels];

            CreateFilter(waveFormat, lowpassCutoffFrequency);
        }
        public override void CalculateRms(int samplesRead, float[] buffer, int offset, int filterOrder)
        {
            throw new NotImplementedException();
        }

        public override float Transform(float sample, int channel)
            => _filters[channel].Transform(sample);

        protected override BiQuadFilter CreateFilter(WaveFormat waveFormat, float lowPassCutoffFrequency)
        {
            for (int channel = 0; channel < waveFormat.Channels; channel++)
            {
                _filters[channel] = BiQuadFilter.LowPassFilter(waveFormat.SampleRate, lowPassCutoffFrequency, 1.0f);
            }

            return null;
        }

        protected override void CreateFilter(WaveFormat waveFormat, float lowPassCutoff, float highPassCutoff, int filterOrder)
        {
            throw new NotImplementedException();
        }

        public override FilterType GetFilterType()
            => FilterType.Lowpass;

        public override void UpdateFilterSettings(float lowpassCutoffFrequency, float highpassCutoffFrequency, int filterOrder)
        {
            CreateFilter(_waveFormat, lowpassCutoffFrequency);
        }
    }
}
