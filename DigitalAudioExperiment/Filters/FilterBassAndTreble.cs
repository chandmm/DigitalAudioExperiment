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
    public class FilterBassAndTreble : FilterAbstractBase, IFilter
    {
        private float _bassGainDB;
        private float _trebleGainDB;
        private float _bassCutoffFrequency = 250f;
        private float _trebleCutoffFrequency = 3000f;

        private BiQuadFilter[] _bassFilters;
        private BiQuadFilter[] _trebleFilters;

        private int _channels;
        private WaveFormat _waveFormat;

        public FilterBassAndTreble(WaveFormat waveFormat, int bass = 0, int treble = 0)
        {
            _waveFormat = waveFormat ?? throw new ArgumentNullException(nameof(waveFormat));
            _channels = _waveFormat.Channels;

            UpdateFilterSettings(bass, treble);
        }

        public override FilterType GetFilterType()
        {
            return FilterType.BassAndTreble;
        }

        public override float Transform(float sample, int channel)
        {
            if (channel < 0 || channel >= _channels)
                throw new ArgumentOutOfRangeException(nameof(channel));

            float bassSample = _bassFilters[channel].Transform(sample);
            float trebleSample = _trebleFilters[channel].Transform(bassSample);

            return trebleSample;
        }

        public override void UpdateFilterSettings(float lowPassCutoffFrequency, float highPassCutoffFrequency, int filterOrder)
        {
            throw new NotSupportedException("Use UpdateFilterSettings(int bass, int treble) for this filter.");
        }

        public override void UpdateFilterSettings(int bassGainDb, int trebleGainDb)
        {
            _bassGainDB = bassGainDb;
            _trebleGainDB = trebleGainDb;

            if (_bassFilters == null || _bassFilters.Length != _channels)
            {
                _bassFilters = new BiQuadFilter[_channels];
                _trebleFilters = new BiQuadFilter[_channels];
            }

            for (int i = 0; i < _channels; i++)
            {
                _bassFilters[i] = BiQuadFilter.LowShelf(
                    _waveFormat.SampleRate,
                    _bassCutoffFrequency,
                    1f,
                    (float)Math.Pow(10, _bassGainDB / 20));

                _trebleFilters[i] = BiQuadFilter.HighShelf(
                    _waveFormat.SampleRate,
                    _trebleCutoffFrequency,
                    1f,
                    (float)Math.Pow(10, _trebleGainDB / 20));
            }
        }

        protected override BiQuadFilter CreateFilter(WaveFormat waveFormat, float lowPassCutoff)
        {
            throw new NotSupportedException("CreateFilter is not used in FilterBassAndTreble.");
        }

        protected override void CreateFilter(WaveFormat waveFormat, float lowPassCutoff, float highPassCutoff, int filterOrder)
        {
            throw new NotSupportedException("CreateFilter is not used in FilterBassAndTreble.");
        }

        protected override void Dispose(bool isDisposing)
        {
            if (!_isDisposed)
            {
                if (isDisposing)
                {
                    _bassFilters = null;
                    _trebleFilters = null;
                }

                _isDisposed = true;
            }
        }
    }
}