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
using Microsoft.Win32;
using System.Windows;

namespace DigitalAudioExperiment.View
{
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            if (DataContext is SettingsViewModel viewModel)
            {
                viewModel.SetOpenFileDialog(GetFiles);
            }
        }

        private string[] GetFiles(string filter)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Multiselect = true,
                Filter = filter
            };

            var dialogResult = openFileDialog.ShowDialog();

            if (dialogResult != null
                && dialogResult == true)
            {
                return openFileDialog.FileNames;
            }

            return null;
        }
    }
}
