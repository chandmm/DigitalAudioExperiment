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
using DigitalAudioExperiment.Infrastructure;
using DigitalAudioExperiment.Model;
using DigitalAudioExperiment.View;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;

namespace DigitalAudioExperiment.ViewModel
{
    public class PlaylistPageViewModel : BaseViewModel
    {
        #region Fields

        private readonly string _fileFiltersAudio = "Mp3 Files (*.mp3)|*.mp3|Playlist Files (*.xml)|*.xml|All files (*.*)|*.*";
        private readonly string _fileFiltersPlaylist = "Playlist Files (*.xml)|*.xml|All files (*.*)|*.*";
        private readonly string _saveFileFilters = "Playlist Files (*.xml)|*.xml";

        private bool _isDisposed;
        private PlaylistModel? _listSelectedPlayModel = null;
        private PlaylistModel? _currentlyPlaying;
        private Func<string, string[]> _getFileCallback;
        private Func<string, string, string?> _getSaveFilePathCallback;
        private Action _closeAction;

        #endregion

        #region Properties

        public bool IsShowing { get; set; }

        public bool IsLoop { get; private set; }

        public bool IsHasList { get => PlayList.Any(); }

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

        #region Commands

        public RelayCommand RemoveCommand { get; private set; }
        public RelayCommand AddFileCommand { get; private set; }
        public RelayCommand SavePlaylistCommand { get; private set; }
        public RelayCommand LoadPlaylistCommand { get; private set; }
        public RelayCommand ExitPlaylistCommand { get; private set; }

        #endregion

        #region Initialisation

        public PlaylistPageViewModel()
        {
            CurrentPlayIndex = 0;
            PlayList = new ObservableCollection<PlaylistModel>();

            RemoveCommand = new RelayCommand(Remove, () => true);
            AddFileCommand = new RelayCommand(AddFile, () => true);
            SavePlaylistCommand = new RelayCommand(SavePlaylist, () => true);
            LoadPlaylistCommand = new RelayCommand(LoadPlaylist, () => true);
            ExitPlaylistCommand = new RelayCommand(ExitPlaylist, () => true);
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

            OnPropertyChanged(nameof(IsHasList));
        }

        public string GetNextFile()
        {
            if (!PlayList.Any())
            {
                return string.Empty;
            }

            if (_listSelectedPlayModel == null
                && PlayList.IndexOf(_currentlyPlaying) == PlayList.Count() - 1)
            {
                return string.Empty; 
            }

            var index = PlayList.IndexOf(_listSelectedPlayModel ?? _currentlyPlaying);

            if (index < 0)
            {
                index = 0;
            }
            else if (_listSelectedPlayModel == null)
            {
                index++;
            }

            _listSelectedPlayModel = null;
            _currentlyPlaying = PlayList.ElementAt(index);
            HandleIsSelected(_currentlyPlaying, updateListSelected: false);

            return _currentlyPlaying.FullFilePathName;
        }


        public string GetNextFile2()
        {
            _currentlyPlaying = _listSelectedPlayModel ?? PlayList
                .Select(x => x)
                .Except(new[] { _currentlyPlaying })?
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

            _currentlyPlaying = _currentlyPlaying;

            CurrentPlayIndex = _currentlyPlaying.SequenceId;

            return _currentlyPlaying.FullFilePathName;
        }

        public void SetLoopPlay(bool loop)
            => IsLoop = loop;

        private void HandleIsSelected(PlaylistModel model, bool updateListSelected = true)
        {
            foreach (var item in PlayList)
            {
                item.IsSelected = false;
            }

            PlayList.ElementAt(_playList.IndexOf(model)).IsSelected = true;

            _listSelectedPlayModel = updateListSelected ? model : null;

            PlayList.Refresh();
        }

        public string GetCurrentlyPlaying()
            => _currentlyPlaying.FullFilePathName;

        private void Remove()
        {
            var item = _playList.FirstOrDefault(x => x.IsSelected);

            if (!IsHasList
                || item == null)
            {
                return;
            }

            _playList.RemoveAt(_playList.IndexOf(item));
            _currentlyPlaying = null;
            _listSelectedPlayModel = null;

            UpdatePlaylistIndices();

            OnPropertyChanged(nameof(IsHasList));
        }

        private void UpdatePlaylistIndices()
        {
            for (int i = 0; i < PlayList.Count(); i++)
            {
                PlayList[i].UpdateSequenceId(i);
            }
        }

        public bool IsCurrentPlayAvailable()
            => _currentlyPlaying != null;

        public void SetGetFileCallback(Func<string, string[]> callback)
        {
            _getFileCallback = callback;
        }

        public void SetGetSaveFileCallback(Func<string, string, string> callback)
        {
            _getSaveFilePathCallback = callback;
        }

        private void AddFile()
        {
            if (_getFileCallback == null)
            {
                return;
            }

            var files = _getFileCallback(_fileFiltersAudio);

            if (files == null
                || !files.Any())
            {
                return;
            }

            foreach (var fileName in files)
            {
                Add(fileName);
            }

            if (!PlayList.Any(x => x.IsSelected))
            {
                PlayList.First().IsSelected = true;
            }
        }

        private void SavePlaylist()
        {
            if (_getSaveFilePathCallback == null)
            {
                return;
            }

            var filePath = _getSaveFilePathCallback(".xml", _saveFileFilters);

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            var serialiser = new XmlSerializer(typeof(List<string>));

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                serialiser.Serialize(fs, PlayList.Select(x => x.FullFilePathName).ToList());
            }
        }

        private void LoadPlaylist()
        {
            if (_getFileCallback == null)
            {
                return;
            }

            var filePath = _getFileCallback(_fileFiltersPlaylist)
                ?.Where(x => x.EndsWith(".xml"))
                .Select(x => x)
                .First();

            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            PlayList.Clear();

            XmlSerializer serializer = new XmlSerializer(typeof(List<string>));

            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                List<string> fullPaths = (List<string>)serializer.Deserialize(fs);

                fullPaths.ForEach(path => Add(path));
            }

            if (PlayList.Any())
            {
                PlayList.First().IsSelected = true;
            }
        }

        public void SetPlaylistViewControl(Action closeAction)
        {
            _closeAction = closeAction;
        }

        public bool IsEndOfList()
            => (PlayList.Count() - 1) == PlayList.IndexOf(_currentlyPlaying);

        private void ExitPlaylist()
        {
            if (_closeAction == null)
            {
                return; 
            }

            _closeAction();
        }

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
