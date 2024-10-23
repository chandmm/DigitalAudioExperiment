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
using DigitalAudioExperiment.Filters;
using NAudio.Dsp;
using NAudio.Wave;

namespace DigitalAudioExperiment.Logic
{
    public class SampleAggregator : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly int _channels;
        private int _notificationCount;
        private int _count;
        private double[] _sumSquares;
        private IFilter _filter;

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
        public SampleAggregator(ISampleProvider source, IFilter filter)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            WaveFormat = source.WaveFormat;
            _channels = WaveFormat.Channels;
            _sumSquares = new double[_channels];

            _filter = filter;
        }

        public SampleAggregator(ISampleProvider source, FilterAbstractBase filter)
        {
            _source = source;
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
                        _sumSquares[0] += Math.Pow(_filter.Transform(buffer[samplesProcessed], 0), 2);
                    }

                    if (_channels == 2
                        && samplesProcessed + 1 < samplesRead)
                    {
                        _sumSquares[1] += Math.Pow(_filter.Transform(buffer[samplesProcessed + 1], 1), 2);
                    }

                    // Handle for multichannel pcm data.
                    if (_channels > 2)
                    {
                        for (int channel = 2; channel < samplesRead; channel++)
                        {
                            _sumSquares[channel] += Math.Pow(_filter.Transform(buffer[samplesProcessed + channel], channel), 2);
                        }
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

        public void UpdateFilterSettings(float centreFrequency, float bandwidth, int filterOrder)
        {
            switch (_filter.GetFilterType())
            {
                case FilterType.Lowpass:
                    _filter.UpdateFilterSettings(centreFrequency, 0f, 0);
                break;
                case FilterType.Highpass:
                    _filter.UpdateFilterSettings(0f, centreFrequency, 0);
                    break;
                case FilterType.Bandpass:
                case FilterType.ButterworthBandpass:
                    _filter.UpdateFilterSettings(centreFrequency - (bandwidth / 2), centreFrequency + (bandwidth / 2), filterOrder);
                    break;
            }
        }
    }
}
