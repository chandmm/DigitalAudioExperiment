/*
    Digital Audio Experiement: Plays mp3 files and may be others in the future.
    Copyright (C) 2024  Michael Chand

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
using DigitalAudioExperiment.ViewModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace DigitalAudioExperiment.View.Components
{
    public partial class PanelControlView : UserControl
    {
        public PanelControlView()
        {
            InitializeComponent();
        }

        private void SeekSiderControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ReceiverViewModel viewModel)
            {
                viewModel.StartIsSeeking(true);
            }
        }

        private void SeekSiderControl_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is ReceiverViewModel viewModel)
            {
                viewModel.SetSeekValue();
            }
        }
    }
}
