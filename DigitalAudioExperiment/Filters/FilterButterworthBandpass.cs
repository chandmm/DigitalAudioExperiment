using DigitalAudioExperiment.Events;
using NAudio.Dsp;
using NAudio.Wave;
using System.Threading.Channels;

namespace DigitalAudioExperiment.Filters
{
    public class FilterButterworthBandpass : FilterAbstractBase, IFilter
    {
        private readonly int _filterOrder; // Must be a multiple of 2 (e.g., 2, 4, 6)

        private float _lowPassCutoffFrequency;
        private float _highPassCutoffFrequency;
        private WaveFormat _waveFormat;
        private BiQuadFilter[][] _lowPassFilters;
        private BiQuadFilter[][] _highPassFilters;
        private int _channels;
        private double[] _sumSquares;
        private int _count;

        public event EventHandler<RmsEventArgs> RmsCalculated;

        public FilterButterworthBandpass(WaveFormat waveFormat, float lowPassCutoff, float highPassCutoff, int filterOrder = 4)
        {
            _lowPassCutoffFrequency = lowPassCutoff;
            _highPassCutoffFrequency = highPassCutoff;
            _waveFormat = waveFormat;
            _channels = _waveFormat.Channels;
            _filterOrder = filterOrder;
            _sumSquares = new double[_channels];

            if (_filterOrder % 2 != 0)
                throw new ArgumentException("Filter order must be a multiple of 2.", nameof(_filterOrder));

            _filterOrder = filterOrder;

            CreateFilter(waveFormat, lowPassCutoff, highPassCutoff, _filterOrder);
        }

        public override void CalculateRms(int samplesRead, float[] buffer, int offset, int notificationCount)
        {
            for (int samplesProcessed = 0; samplesProcessed < samplesRead; samplesProcessed += _channels)
            {
                if (samplesProcessed < samplesRead)
                {
                    float sample = buffer[samplesProcessed];

                    // Pass the sample through high-pass filter stages
                    foreach (var filter in _highPassFilters[0])
                    {
                        sample = filter.Transform(sample);
                    }

                    // Pass the sample through low-pass filter stages
                    foreach (var filter in _lowPassFilters[0])
                    {
                        sample = filter.Transform(sample);
                    }

                    _sumSquares[0] += Math.Pow(sample, 2);
                }

                if (_channels == 2
                    && samplesProcessed + 1 < samplesRead)
                {
                    float sample = buffer[samplesProcessed + 1];

                    foreach (var filter in _highPassFilters[1])
                    {
                        sample = filter.Transform(sample);
                    }

                    // Pass the sample through low-pass filter stages
                    foreach (var filter in _lowPassFilters[1])
                    {
                        sample = filter.Transform(sample);
                    }

                    _sumSquares[1] += Math.Pow(sample, 2);
                }

                // Handle for multichannel pcm data.
                if (_channels > 2)
                {
                    for (int channel = 2; channel < samplesRead; channel++)
                    {
                        float sample = buffer[samplesProcessed + channel];

                        foreach (var filter in _highPassFilters[channel])
                        {
                            sample = filter.Transform(sample);
                        }

                        // Pass the sample through low-pass filter stages
                        foreach (var filter in _lowPassFilters[channel])
                        {
                            sample = filter.Transform(sample);
                        }

                        _sumSquares[channel] += Math.Pow(sample, 2);
                    }
                    // Accumulate sum of squares of filtered samples
                }

                _count++;

                if (_count >= notificationCount)
                {
                    double[] rmsValues = new double[_channels];

                    for (int channel = 0; channel < _channels; channel++)
                    {
                        rmsValues[channel] = Math.Sqrt(_sumSquares[channel] / _count);
                    }

                    RmsCalculated?.Invoke(this, new RmsEventArgs(rmsValues));

                    _count = 0;
                    Array.Clear(_sumSquares, 0, _sumSquares.Length);
                }
            }
        }

        protected override BiQuadFilter CreateFilter(WaveFormat waveFormat, float lowPassCutoff)
        {
            throw new NotImplementedException();
        }

        protected override void CreateFilter(WaveFormat waveFormat, float lowPassCutoff, float highPassCutoff, int filterOrder)
        {
            // Initialize cascaded high-pass filters for each channel
            int stages = filterOrder / 2;
            _highPassFilters = new BiQuadFilter[_channels][];
            for (int ch = 0; ch < _channels; ch++)
            {
                _highPassFilters[ch] = new BiQuadFilter[stages];
                for (int stage = 0; stage < stages; stage++)
                {
                    // Initialize each stage of the high-pass filter
                    _highPassFilters[ch][stage] = BiQuadFilter.HighPassFilter(waveFormat.SampleRate, _lowPassCutoffFrequency, 1.0f);
                }
            }

            // Initialize cascaded low-pass filters for each channel
            _lowPassFilters = new BiQuadFilter[_channels][];
            for (int ch = 0; ch < _channels; ch++)
            {
                _lowPassFilters[ch] = new BiQuadFilter[stages];
                for (int stage = 0; stage < stages; stage++)
                {
                    // Initialize each stage of the low-pass filter
                    _lowPassFilters[ch][stage] = BiQuadFilter.LowPassFilter(waveFormat.SampleRate, _highPassCutoffFrequency, 1.0f);
                }
            }
        }

        public override float Transform(float sample, int channel)
        {
            float filteredSample = sample;

            foreach (var filter in _highPassFilters[channel])
            {
                filteredSample = filter.Transform(filteredSample);
            }

            foreach (var filter in _lowPassFilters[channel])
            {
                filteredSample = filter.Transform(filteredSample);
            }

            return filteredSample;
        }

        public override FilterType GetFilterType()
            => FilterType.ButterworthBandpass;

        public override void UpdateFilterSettings(float lowepassCutoffFrequency, float highepassCutoffFrequency, int filterOrder)
            => CreateFilter(_waveFormat, lowepassCutoffFrequency, highepassCutoffFrequency, filterOrder);
    }
}
