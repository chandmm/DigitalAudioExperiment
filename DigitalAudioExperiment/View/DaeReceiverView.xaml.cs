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
using System.Windows.Controls;

namespace DigitalAudioExperiment.View
{
    public partial class DaeReceiverView : UserControl
    {
        public DaeReceiverView()
        {
            App.Current.Exit += OnExit;
            InitializeComponent();
            this.Unloaded += OnUnloaded;

            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            
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

        #region Cleanup

        private void OnExit(object sender, ExitEventArgs e)
        {
            if (DataContext is DaeReceiverViewModel viewModel)
            {
                viewModel.Dispose();
            }
        }

        #endregion
    }
}
