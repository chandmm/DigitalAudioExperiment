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
        private PlaylistModel? _currentlyPlaying;
        private Func<string, string[]> _getFileCallback;
        private Func<string, string, string?> _getSaveFilePathCallback;
        private Action _closeAction;
        private int _previousPlayIndex = -1;

        #endregion

        #region Properties

        public bool IsShowing { get; set; }

        public bool IsHasList { get => PlayList.Any(); }

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
                _previousPlayIndex = -1;
                return string.Empty;
            }
            var item = PlayList.FirstOrDefault(x => x.IsSelected);
            
            if (item == null)
            {
                _previousPlayIndex = -1;
            }

            if (item != null
                && _previousPlayIndex == PlayList.Count() - 1)
            {
                return string.Empty;
            }

            item = item != null && item.IsUserSelected ? item : PlayList.ElementAt(_previousPlayIndex + 1 );

            _previousPlayIndex = PlayList.IndexOf(item);

            if (!item.IsUserSelected)
            {
                HandleIsSelected(item);
            }

            item.IsUserSelected = false;

            return item.FullFilePathName;
        }

        private void HandleIsSelected(PlaylistModel model, bool updateListSelected = true)
        {
            foreach (var item in PlayList)
            {
                item.IsSelected = false;
            }

            PlayList.ElementAt(_playList.IndexOf(model)).IsSelected = true;

            if (model.IsUserSelected)
            {
                _previousPlayIndex = -1;
            }

            PlayList.Refresh();
        }

        private void Remove()
        {
            var item = _playList.FirstOrDefault(x => x.IsSelected);

            if (!IsHasList
                || item == null)
            {
                return;
            }

            var itemIndex = _playList.IndexOf(item);
            _playList.RemoveAt(_playList.IndexOf(item));

            if (item.IsSelected
                && itemIndex < PlayList.Count())
            {
                HandleIsSelected(PlayList.ElementAt(itemIndex));
            }

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

            var filePath = _getSaveFilePathCallback(_saveFileFilters, ".xml");

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

        private void ExitPlaylist()
        {
            if (_closeAction == null)
            {
                return; 
            }

            _closeAction();
        }

        public bool IsLastItem()
            => _previousPlayIndex == PlayList.Count() - 1;

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
