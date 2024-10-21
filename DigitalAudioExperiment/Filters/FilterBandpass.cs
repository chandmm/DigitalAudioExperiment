using NAudio.Dsp;
using NAudio.Wave;
using System.Threading.Channels;

namespace DigitalAudioExperiment.Filters
{
    public class FilterBandpass : FilterAbstractBase, IFilter
    {
        private BiQuadFilter[] _filters;
        private int _channels;

        public FilterBandpass(WaveFormat waveFormat, float lowerCutoffFrequency, float upperCutoffFrequency)
        {
            _channels = waveFormat.Channels;
            // Initialize band-pass filters for each channel
            _filters = new BiQuadFilter[_channels];

            
            CreateFilter(waveFormat, lowerCutoffFrequency, upperCutoffFrequency, 0);
        }

        public override void CalculateRms(int samplesRead, float[] buffer, int offset, int filterOrder)
        {
            throw new NotImplementedException();
        }

        public override float Transform(float sample, int channel)
            => _filters[channel].Transform(sample);

        protected override BiQuadFilter CreateFilter(WaveFormat waveFormat, float lowPassCutoff)
        {
            throw new NotImplementedException();
        }

        protected override void CreateFilter(WaveFormat waveFormat, float lowPassCutoffFrequency, float highPassCutoffFrequency, int filterOrder)
        {
            for (int channel = 0; channel < _channels; channel++)
            {
                float centerFrequency = (lowPassCutoffFrequency + highPassCutoffFrequency) / 2.0f;
                float bandwidth = highPassCutoffFrequency - lowPassCutoffFrequency;

                // Calculate Q factor
                float q = centerFrequency / bandwidth;

                // Create band-pass filter with constant peak gain
                _filters[channel] =  BiQuadFilter.BandPassFilterConstantPeakGain(waveFormat.SampleRate, centerFrequency, q);
            }
        }
    }
}
