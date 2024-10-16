﻿/*
    Digital Audio Experiement: Plays mp3 files and may be others in the future.
    Copyright (C) 2024  Michael Chand

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
using NAudio.Wave;

namespace DigitalAudioExperiment.Logic
{
    public class SampleAggregator : ISampleProvider
    {
        private readonly ISampleProvider source;
        private readonly int channels;
        private int notificationCount;
        private int count;
        private double[] sumSquares; // Sum of squares for each channel

        public WaveFormat WaveFormat { get; }

        public int NotificationCount
        {
            get => notificationCount;
            set => notificationCount = value;
        }

        public bool PerformRmsCalculation { get; set; }

        public event EventHandler<RmsEventArgs> RmsCalculated;

        public SampleAggregator(ISampleProvider source)
        {
            this.source = source;
            this.WaveFormat = source.WaveFormat;
            this.channels = source.WaveFormat.Channels;
            this.sumSquares = new double[channels];
        }

        public int Read(float[] buffer, int offset, int sampleCount)
        {
            int samplesRead = source.Read(buffer, offset, sampleCount);

            if (PerformRmsCalculation)
            {
                int samplesProcessed = 0;

                while (samplesProcessed < samplesRead)
                {
                    for (int ch = 0; ch < channels; ch++)
                    {
                        if (samplesProcessed + ch < samplesRead)
                        {
                            float sample = buffer[offset + samplesProcessed + ch];
                            sumSquares[ch] += sample * sample;
                        }
                    }

                    samplesProcessed += channels;
                    count++;

                    if (count >= notificationCount)
                    {
                        // Calculate RMS for each channel
                        double[] rmsValues = new double[channels];
                        for (int ch = 0; ch < channels; ch++)
                        {
                            double meanSquare = sumSquares[ch] / count;
                            rmsValues[ch] = Math.Sqrt(meanSquare);
                        }

                        RmsCalculated?.Invoke(this, new RmsEventArgs(rmsValues));

                        // Reset counters for the next block
                        count = 0;
                        Array.Clear(sumSquares, 0, sumSquares.Length);
                    }
                }
            }

            return samplesRead;
        }
    }

    public class RmsEventArgs : EventArgs
    {
        public double[] RmsValues { get; }

        public RmsEventArgs(double[] rmsValues)
        {
            RmsValues = rmsValues;
        }
    }
}
