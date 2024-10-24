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
            => _filters[channel].Transform(sample);

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
