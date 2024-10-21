using DigitalAudioExperiment.Events;
using NAudio.Dsp;
using NAudio.Wave;

namespace DigitalAudioExperiment.Filters
{
    public abstract class FilterAbstractBase : IFilter
    {
        public event EventHandler<RmsEventArgs> RmsCalculated;

        public abstract void CalculateRms(int samplesRead, float[] buffer, int offset, int filterOrder);
        public abstract float Transform(float sample, int channel);
        protected abstract BiQuadFilter CreateFilter(WaveFormat waveFormat, float lowPassCutoff);
        protected abstract void CreateFilter(WaveFormat waveFormat, float lowPassCutoff, float highPassCutoff, int filterOrder);
    }
}
