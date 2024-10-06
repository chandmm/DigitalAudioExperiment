using System.Collections.ObjectModel;
using System.Windows.Data;

namespace DigitalAudioExperiment.Extensions
{
    public static class ContainerExtensions
    {
        public static void Refresh<T>(this ObservableCollection<T> value)
        {
            CollectionViewSource.GetDefaultView(value).Refresh();
        }
    }
}
