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
using DigitalAudioExperiment.Events;
using NAudio.Dsp;
using NAudio.Wave;

namespace DigitalAudioExperiment.Filters
{
    public abstract class FilterAbstractBase : IFilter, IDisposable
    {
        protected bool _isDisposed;

        public event EventHandler<RmsEventArgs>? RmsCalculated;

        public abstract FilterType GetFilterType();
        public abstract float Transform(float sample, int channel);
        public abstract void UpdateFilterSettings(float lowepassCutoffFrequency, float highepassCutoffFrequency, int filterOrder);
        protected abstract BiQuadFilter CreateFilter(WaveFormat waveFormat, float lowPassCutoff);
        protected abstract void CreateFilter(WaveFormat waveFormat, float lowPassCutoff, float highPassCutoff, int filterOrder);

        protected abstract void Dispose(bool isDisposing);

        public virtual void UpdateFilterSettings(int valueOne, int valueTwo)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}
