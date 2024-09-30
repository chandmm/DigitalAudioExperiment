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
using DigitalAudioExperiment.Extensions;
using DigitalAudioExperiment.Model;
using System.Collections.ObjectModel;
using System.IO;

namespace DigitalAudioExperiment.ViewModel
{
    public class PlaylistPageViewModel : BaseViewModel
    {
        #region Fields

        private bool _isDisposed;
        private PlaylistModel? _listSelectedPlayModel = null;
        private PlaylistModel? _currentlyPlaying;

        #endregion

        #region Properties

        public bool IsShowing { get; set; }

        public bool IsLoop { get; private set; }

        private int _currentPlayIndex;
        public int CurrentPlayIndex
        {
            get => _currentPlayIndex;
            set
            {
                _currentPlayIndex = value;

                OnPropertyChanged();
            }
        }

        private ObservableCollection<PlaylistModel> _playList;
        public ObservableCollection<PlaylistModel> PlayList
        {
            get => _playList;
            set
            {
                _playList = value;

                OnPropertyChanged();
            }
        }

        #endregion

        #region Initialisation

        public PlaylistPageViewModel()
        {
            CurrentPlayIndex = 0;
            PlayList = new ObservableCollection<PlaylistModel>();
        }

        #endregion

        #region Manage List

        public void Add(string playlistItem)
        {
            PlayList.Add(
                new PlaylistModel(
                    PlayList.Any() ? PlayList.Max(x => x.SequenceId) : 0,
                    HandleIsSelected)
                {
                    FullFilePathName = playlistItem,
                    FileName = Path.GetFileName(playlistItem)
                });
        }

        public void Remove(string playlistItem)
        {
            var item = PlayList.FirstOrDefault(x => x.FullFilePathName == playlistItem);

            if (item != null)
            {
                PlayList.Remove(item);
            }
        }

        public string GetNextFile()
        {
            _currentlyPlaying = _listSelectedPlayModel ?? PlayList
                .Select(x => x)
                .Except(new[] {_currentlyPlaying })?
                .FirstOrDefault(x => x.SequenceId > CurrentPlayIndex);


            if (_currentlyPlaying == null)
            {
                return string.Empty;
            }

            HandleIsSelected(_currentlyPlaying);

            _listSelectedPlayModel = null;

            if (IsLoop
                && CurrentPlayIndex == _currentlyPlaying.SequenceId)
            {
                CurrentPlayIndex = PlayList.First().SequenceId;

                return PlayList.First().FullFilePathName;
            }

            //if (!IsLoop)
            //{
            //    PlayList.RemoveAt(PlayList.IndexOf(model));
            //}

            _currentlyPlaying = _currentlyPlaying;

            CurrentPlayIndex = _currentlyPlaying.SequenceId;

            return _currentlyPlaying.FullFilePathName;
        }

        public void SetLoopPlay(bool loop)
            => IsLoop = loop;

        private void HandleIsSelected(PlaylistModel model)
        {
            foreach (var item in PlayList)
            {
                item.IsSelected = false;
            }

            PlayList.ElementAt(_playList.IndexOf(model)).IsSelected = true;

            _listSelectedPlayModel = model;

            PlayList.Refresh();
        }

        public string GetCurrentlyPlaying()
            => _currentlyPlaying.FullFilePathName;

        #endregion

        #region Dispose

        protected override void Dispose(bool isDisposng)
        {
            if (!_isDisposed)
            {
                if (isDisposng)
                {
                    PlayList.Clear();
                    PlayList = null;
                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
