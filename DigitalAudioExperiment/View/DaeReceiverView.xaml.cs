using DigitalAudioExperiment.ViewModel;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls;

namespace DigitalAudioExperiment.View
{
    public partial class DaeReceiverView : UserControl
    {
        public DaeReceiverView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            OnDataContextChanged(sender, default(DependencyPropertyChangedEventArgs));
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (DataContext is DaeReceiverViewModel viewModel)
            {
                viewModel.SetGetFileCallback(GetFile);
            }
        }

        private string? GetFile()
        {
            var openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Mp3 Files|*.mp3";
            
            var dialogResult = openFileDialog.ShowDialog();
            var fileName = openFileDialog.FileName;

            if (dialogResult != null
                && dialogResult == true
                && !string.IsNullOrEmpty(fileName))
            {
                return fileName;
            }

            return null;
        }
    }
}
