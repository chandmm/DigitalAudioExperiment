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
using DigitalAudioExperiment.ViewModel;
using Microsoft.Win32;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace DigitalAudioExperiment.View
{
    public partial class PlaylistPageView : Window
    {
        public PlaylistPageView()
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
            if (DataContext is PlaylistPageViewModel viewModel)
            {
                viewModel.SetGetFileCallback(GetFile);
                viewModel.SetGetSaveFileCallback(GetSaveFile);
            }
        }

        private string[] GetFile(string filter)
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

        private string GetSaveFile(string filters, string defaultExtension)
        {
            var saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = filters;
            saveFileDialog.FileName = "DaePlaylist";
            saveFileDialog.DefaultExt = defaultExtension;

            var dialogResult = saveFileDialog.ShowDialog();

            if (dialogResult != null
                && dialogResult == true)
            {
                return saveFileDialog.FileName;
            }

            return null;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            this.DragMove();
        }

        public new void Show()
        {
            if (DataContext is PlaylistPageViewModel viewModel)
            {
                viewModel.IsShowing = true;

                if (base.Visibility == Visibility.Collapsed)
                {
                    base.Visibility = Visibility.Visible;

                    return;
                }

                base.Show();
            }
        }

        public new void Close()
        {
            if (DataContext is PlaylistPageViewModel viewModel)
            {
                viewModel.IsShowing = false;

                base.Visibility = Visibility.Collapsed;
            }
        }

        public void CloseExit()
            => base.Close();
    }
}
