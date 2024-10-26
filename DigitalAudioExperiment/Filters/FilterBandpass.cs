/*
    Digital Audio Experiement: Plays mp3 files and may be others in the future.
    Copyright (C) 2024  Michael Chand.

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
using NAudio.Dsp;
using NAudio.Wave;

namespace DigitalAudioExperiment.Filters
{
    public class FilterBandpass : FilterAbstractBase, IFilter
    {
        private BiQuadFilter[] _filters;
        private int _channels;
        private WaveFormat _waveFormat;

        public FilterBandpass(WaveFormat waveFormat, float lowerCutoffFrequency, float upperCutoffFrequency)
        {
            _waveFormat = waveFormat;
            _channels = _waveFormat.Channels;
            // Initialize band-pass filters for each channel
            _filters = new BiQuadFilter[_channels];

            
            CreateFilter(_waveFormat, lowerCutoffFrequency, upperCutoffFrequency, 0);
        }

        public override float Transform(float sample, int channel)
            => _filters[channel].Transform(sample);

        protected override BiQuadFilter CreateFilter(WaveFormat waveFormat, float lowPassCutoff)
        {
            throw new NotImplementedException();
        }

        protected override void CreateFilter(WaveFormat waveFormat, float lowerCutoffFrequency, float upperCutoffFrequency, int filterOrder)
        {
            float centerFrequency = (lowerCutoffFrequency + upperCutoffFrequency) / 2.0f;
            float q = centerFrequency / (upperCutoffFrequency - lowerCutoffFrequency);

            for (int channel = 0; channel < _channels; channel++)
            {
                // Create band-pass filter with constant peak gain
                _filters[channel] =  BiQuadFilter.BandPassFilterConstantPeakGain(waveFormat.SampleRate, centerFrequency, q);
            }
        }

        public override void UpdateFilterSettings(float lowepassCutoffFrequency, float highepassCutoffFrequency, int filterOrder)
            => CreateFilter(_waveFormat, lowepassCutoffFrequency, highepassCutoffFrequency, filterOrder);

        public override FilterType GetFilterType()
            => FilterType.Bandpass;

        protected override void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    for (int i = 0; i < _filters.Count(); i++)
                    {
                        _filters[i] = null;
                    }

                    _filters = null;
                }

                _isDisposed = true;
            }
        }
    }
}
