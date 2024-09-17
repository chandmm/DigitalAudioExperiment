using DigitalAudioExperiment.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace DigitalAudioExperiment.View.Components
{
    /// <summary>
    /// Interaction logic for PanelControlView.xaml
    /// </summary>
    public partial class PanelControlView : UserControl
    {
        public PanelControlView()
        {
            InitializeComponent();
        }

        private void SeekSiderControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DaeReceiverViewModel viewModel)
            {
                viewModel.StartIsSeeking(true);
            }
        }

        private void SeekSiderControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is DaeReceiverViewModel viewModel)
            {
                viewModel.SetSeekValue();
            }
        }
    }
}
