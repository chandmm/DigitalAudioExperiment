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
using NAudio.Wave;

namespace DigitalAudioExperiment.Filters
{
    public enum FilterType
    {
        Lowpass,
        Highpass,
        Bandpass,
        ButterworthBandpass
    }

    public static class FilterFactory
    {
        public static IFilter GetFilterInterface(FilterType filterType, WaveFormat waveFormat, float lowpassCutoffFrequency, float highpassCutoffFrequency, int filterORder)
        {
            switch (filterType)
            {
                case FilterType.Lowpass:
                    return new FilterLowpass(waveFormat, lowpassCutoffFrequency);
                case FilterType.Highpass:
                    return new FilterHighpass(waveFormat, highpassCutoffFrequency);
                case FilterType.Bandpass:
                    return new FilterBandpass(waveFormat, lowpassCutoffFrequency, highpassCutoffFrequency);
                case FilterType.ButterworthBandpass:
                    return new FilterButterworthBandpass(waveFormat, lowpassCutoffFrequency, highpassCutoffFrequency);
            }

            throw new NotImplementedException();
        }
    }
}
