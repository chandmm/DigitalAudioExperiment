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
using DigitalAudioExperiment.Pages;
using DigitalAudioExperiment.ViewModel;
using Microsoft.Win32;
using System.Drawing;
using System.Windows;
using System.Windows.Input;

namespace DigitalAudioExperiment.View
{
    public partial class PlaylistPageView : Window
    {
        private bool _isVisible;
        private int _dockDetectionThreashold = 50;
        private bool _locationChanging;

        public PlaylistPageView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
            Loaded += OnLoaded;
            LocationChanged += OnLocationChanged;
            PreviewMouseUp += OnPreviewMouseUp;
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
                viewModel.SetPlaylistViewControl(Close);
                Owner = MainWindow.Instance;
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

        private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs args)
        {
            _isVisible = Visibility == Visibility.Visible;
        }

        public bool GetIsViewVisible()
            => _isVisible;

        public void SetPlaylistPageComponent(PlaylistPage page)
        {
            playlistPageComponent = page;
        }

        public PlaylistPage GetPlaylistPage()
            => playlistPageComponent;

        #region Docking/Undocking

        private void OnLocationChanged(object? sender, EventArgs args)
        {
            _locationChanging = true;

            var viewModel = this.DataContext as PlaylistPageViewModel;

            if (viewModel != null
                && !viewModel.IsDocked
                && IsInsideDockingRange())
            {
                viewModel.IsCanDock = true;
            }
            else
            {
                viewModel.IsCanDock = false;
            }

        }

        private bool IsInsideDockingRange()
        {
            var mainWindowBoundsRight = new Rectangle(
                (int)(MainWindow.Instance.Left),
                (int)MainWindow.Instance.Top - _dockDetectionThreashold,
                (int)MainWindow.Instance.ActualWidth,
                (int)MainWindow.Instance.ActualHeight + _dockDetectionThreashold);
            var bounds = new Rectangle((int)Left, 
                (int)Top,
                (int)_dockDetectionThreashold,
                (int)MainWindow.Instance.ActualHeight);

            if (bounds.Left <= mainWindowBoundsRight.Right 
                && bounds.Left >= (mainWindowBoundsRight.Right - _dockDetectionThreashold)
                && bounds.Top >= mainWindowBoundsRight.Top
                && bounds.Top <= mainWindowBoundsRight.Bottom)
            {
                return true;
            }

           return false;
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs args)
        {
            var viewModel = DataContext as PlaylistPageViewModel;

            if (viewModel != null
                && !viewModel.IsDocked
                && _locationChanging
                && IsInsideDockingRange())
            {
                viewModel.IsDocked = true;
                viewModel.IsCanDock = false;
            }

            _locationChanging = false;
        }

        internal void SetDocked()
        {
            if (DataContext is PlaylistPageViewModel viewModel)
            {
                viewModel.IsDocked = true;
            }
        }

        #endregion

        public new void Close()
        {
            if (DataContext is PlaylistPageViewModel viewModel)
            {
                base.Visibility = Visibility.Collapsed;
            }
        }

        public void CloseExit()
        {
            DataContextChanged -= OnDataContextChanged;
            LocationChanged -= OnLocationChanged;
            PreviewMouseUp -= OnPreviewMouseUp;

            base.Close();
        }
    }
}
