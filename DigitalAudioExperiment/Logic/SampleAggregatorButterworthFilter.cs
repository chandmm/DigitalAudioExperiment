/*
    Digital Audio Experiement: Plays mp3 files and may be others in the future.
    Copyright (C) 2024  Michael Chand

    This class is a data stream intercepter so that samples being played
    can be used for calculating db values. This class returns the calculated RMS
    component part.

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
using DigitalAudioExperiment.Events;
using NAudio.Dsp;
using NAudio.Wave;

namespace DigitalAudioExperiment.Logic
{
    public class SampleAggregatorButterworthFilter : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly int _channels;
        private int _notificationCount;
        private int _count;
        private double[] _sumSquares;

        // Arrays of filters for cascading (one per channel)
        private BiQuadFilter[][] _lowPassFilters;
        private BiQuadFilter[][] _highPassFilters;

        // Filter parameters
        private readonly float _lowCutoffFrequency;
        private readonly float _highCutoffFrequency;
        private readonly int _filterOrder; // Must be a multiple of 2 (e.g., 2, 4, 6)

        public WaveFormat WaveFormat { get; }

        public int NotificationCount
        {
            get => _notificationCount;
            set => _notificationCount = value;
        }

        public bool PerformRmsCalculation { get; set; }

        public event EventHandler<RmsEventArgs> RmsCalculated;

        /// <summary>
        /// Initializes a new instance of the SampleAggregator class with a Butterworth band-pass filter.
        /// </summary>
        /// <param name="source">The source ISampleProvider.</param>
        /// <param name="lowCutoffFrequency">The lower cutoff frequency for the band-pass filter.</param>
        /// <param name="highCutoffFrequency">The upper cutoff frequency for the band-pass filter.</param>
        /// <param name="filterOrder">The order of the Butterworth filter (must be a multiple of 2).</param>
        public SampleAggregatorButterworthFilter(ISampleProvider source, float lowCutoffFrequency, float highCutoffFrequency, int filterOrder = 4)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            WaveFormat = source.WaveFormat;
            _channels = WaveFormat.Channels;
            _sumSquares = new double[_channels];

            _lowCutoffFrequency = lowCutoffFrequency;
            _highCutoffFrequency = highCutoffFrequency;
            _filterOrder = filterOrder;

            // Ensure filterOrder is a multiple of 2
            if (_filterOrder % 2 != 0)
                throw new ArgumentException("Filter order must be a multiple of 2.", nameof(filterOrder));

            // Initialize cascaded high-pass filters for each channel
            int stages = _filterOrder / 2;
            _highPassFilters = new BiQuadFilter[_channels][];
            for (int ch = 0; ch < _channels; ch++)
            {
                _highPassFilters[ch] = new BiQuadFilter[stages];
                for (int stage = 0; stage < stages; stage++)
                {
                    // Initialize each stage of the high-pass filter
                    _highPassFilters[ch][stage] = BiQuadFilter.HighPassFilter(WaveFormat.SampleRate, _lowCutoffFrequency, 1.0f);
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
                    _lowPassFilters[ch][stage] = BiQuadFilter.LowPassFilter(WaveFormat.SampleRate, _highCutoffFrequency, 1.0f);
                }
            }
        }

        public int Read(float[] buffer, int offset, int sampleCount)
        {
            int samplesRead = _source.Read(buffer, offset, sampleCount);

            Task.Run(() => CalculateRms(samplesRead, buffer, offset))
                .ConfigureAwait(false);

            return samplesRead;
        }

        private void CalculateRms(int samplesRead, float[] buffer, int offset)
        {
            if (PerformRmsCalculation)
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

                    if (_count >= _notificationCount)
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
        }

        private BiQuadFilter CreateBandPassFilter(int sampleRate, float lowerCutoff, float upperCutoff)
        {
            // Calculate center frequency and bandwidth
            float centerFrequency = (lowerCutoff + upperCutoff) / 2.0f;
            float bandwidth = upperCutoff - lowerCutoff;

            // Calculate Q factor
            float q = centerFrequency / bandwidth;

            // Create band-pass filter with constant peak gain
            return BiQuadFilter.BandPassFilterConstantPeakGain(sampleRate, centerFrequency, q);
        }
    }
}
