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
using System;

namespace DigitalAudioExperiment.Logic
{
    public class SampleAggregator : ISampleProvider
    {
        private readonly ISampleProvider _source;
        private readonly int _channels;
        private int _notificationCount;
        private int _count;
        private double[] _sumSquares;

        private BiQuadFilter[] _filters;

        public WaveFormat WaveFormat { get; }

        public int NotificationCount
        {
            get => _notificationCount;
            set => _notificationCount = value;
        }

        public bool PerformRmsCalculation { get; set; }

        public event EventHandler<RmsEventArgs> RmsCalculated;

        public SampleAggregator(ISampleProvider source, float lowerCutoffFrequency, float upperCutoffFrequency)
        {
            _source = source;
            WaveFormat = source.WaveFormat;
            _channels = WaveFormat.Channels;
            _sumSquares = new double[_channels];

            // Initialize band-pass filters for each channel
            _filters = new BiQuadFilter[_channels];
            for (int ch = 0; ch < _channels; ch++)
            {
                _filters[ch] = CreateBandPassFilter(WaveFormat.SampleRate, lowerCutoffFrequency, upperCutoffFrequency);
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
                        _sumSquares[0] += Math.Pow(_filters[0].Transform(buffer[samplesProcessed]), 2);
                    }

                    if (_channels == 2
                        && samplesProcessed + 1 < samplesRead)
                    {
                        _sumSquares[1] += Math.Pow(_filters[1].Transform(buffer[samplesProcessed]), 2);
                    }

                    // Handle for multichannel pcm data.
                    if (_channels > 2)
                    {
                        for (int channel = 2; channel < samplesRead; channel++)
                        {
                            _sumSquares[channel] += Math.Pow(_filters[channel].Transform(buffer[samplesProcessed]), 2);
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
