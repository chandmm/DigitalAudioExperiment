namespace DigitalAudioExperiment.Events
{
    public class RmsEventArgs : EventArgs
    {
        public double[] RmsValues { get; }

        public RmsEventArgs(double[] rmsValues)
        {
            RmsValues = rmsValues;
        }
    }
}
