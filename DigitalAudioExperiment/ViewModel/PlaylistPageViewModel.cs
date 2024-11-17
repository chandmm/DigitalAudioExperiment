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
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;

namespace DigitalAudioExperiment.ViewModel
{
    public class PlaylistPageViewModel : BaseViewModel
    {
        #region Fields

        public const string FileFiltersAudio = "Audio Files |*.mp3;*.flac;*.wav|Mp3 Files (*.mp3)|*.mp3|Flac Files (*.flac)|*.flac|Wave Files (*.wav)|*.wav|Playlist Files (*.xml)|*.xml|All files (*.*)|*.*";

        private readonly string _fileFiltersPlaylist = "Playlist Files (*.xml)|*.xml|All files (*.*)|*.*";
        private readonly string _saveFileFilters = "Playlist Files (*.xml)|*.xml";

        private bool _isDisposed;
        private Func<string, string[]> _getFileCallback;
        private Func<string, string, string?> _getSaveFilePathCallback;
        private Action _closeAction;
        private int _previousPlayIndex = -1;
        private string? _currentPlaylist;

        public delegate void DockingChanged(bool isDocked);
        public event DockingChanged DockingChangedEvent;

        #endregion

        #region Properties

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

        public bool IsShowing { get; set; }

        private bool _isDocked;
        public bool IsDocked 
        {
            get => _isDocked;
            set
            { 
                _isDocked = value;

                DockingChangedEvent?.Invoke(_isDocked);
                OnPropertyChanged();
            }
        }

        private bool _isCanDock;
        public bool IsCanDock
        {
            get => _isCanDock;
            set
            {
                _isCanDock = value;

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
        public RelayCommand RemoveAllCommand { get; private set; }
        public RelayCommand MoveUpCommand { get; private set; }
        public RelayCommand MoveDownCommand { get; private set; }
        public RelayCommand DockToggleCommand { get; private set; }

        #endregion

        #region Initialisation

        public PlaylistPageViewModel()
        {
            PlayList = new ObservableCollection<PlaylistModel>();

            RemoveAllCommand = new RelayCommand(RemoveAll, () => true);
            RemoveCommand = new RelayCommand(Remove, () => true);
            AddFileCommand = new RelayCommand(AddFile, () => true);
            SavePlaylistCommand = new RelayCommand(SavePlaylist, () => true);
            LoadPlaylistCommand = new RelayCommand(() => LoadPlaylist(), () => true);
            MoveUpCommand = new RelayCommand(() => MoveItem(false), () => true);
            MoveDownCommand = new RelayCommand(MoveItemDown, () => true);
            DockToggleCommand = new RelayCommand(DockToggle, () => true);

            ExitPlaylistCommand = new RelayCommand(ExitPlaylist, () => true);
        }

        #endregion

        #region Manage List

        public void Add(string playlistItem)
        {
            PlayList.Add(
                new PlaylistModel(HandleIsSelected)
                {
                    FullFilePathName = playlistItem,
                    FileName = Path.GetFileName(playlistItem)
                });

            OnPropertyChanged(nameof(IsHasList));
        }

        public string GetNextFile()
        {
            if (!IsHasList)
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

                if (item.FullFilePathName != model.FullFilePathName)
                {
                    item.IsUserSelected = false;
                }
            }

            model.IsSelected = true;

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

            OnPropertyChanged(nameof(IsHasList));
        }

        public void RemoveAll()
        {
            if (!IsHasList)
            {
                return;
            }

            PlayList.Clear();

            _currentPlaylist = null;

            OnPropertyChanged(nameof(IsHasList));
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

            var files = _getFileCallback(FileFiltersAudio);

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

        private void LoadPlaylist(string? fileName = null)
        {
            if (_getFileCallback == null
                && string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var filePath = (string.IsNullOrEmpty(fileName) ? _getFileCallback(_fileFiltersPlaylist) : new[] { fileName })
                ?.Where(x => x.EndsWith(".xml"))
                .Select(x => x)
                .First();

            LoadPlaylistFromFile(filePath);
        }

        public void LoadPlaylistFile(string? fileName)
            => LoadPlaylist(fileName);

        public void LoadPlaylistFromFile(string filePath)
        {
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

            if (IsHasList)
            {
                PlayList.First().IsSelected = true;
                ResetToSelectedPlayed();
            }

            _currentPlaylist = filePath;
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
            IsShowing = false;

            Update();
        }

        public bool IsLastItem()
            => _previousPlayIndex == PlayList.Count() - 1;

        public void ResetToSelectedPlayed()
        {
            var item = PlayList.FirstOrDefault(x => x.IsSelected);

            if (item == null)
            {
                return;

            }
            
            _previousPlayIndex = PlayList.IndexOf(item) - 1;
        }

        private void MoveItem(bool isDown = false)
        {
            if (PlayList == null
                || !IsHasList)
            {
                return;
            }

            var items = PlayList.Where(x => x.IsSelected || x.IsUserSelected).Select(x => x);

            if (items == null
                || !items.Any())
            {
                return;
            }

            var item = items.Count() > 1 ? items.FirstOrDefault(x => x.IsUserSelected) : items.FirstOrDefault();

            if (item == null)
            {
                return;
            }

            var index = PlayList.IndexOf(item);

            if (isDown)
            {
                if (index == PlayList.Count() - 1)
                {
                    return;
                }

                PlayList.RemoveAt(index);
                PlayList.Insert(index + 1, item);

                _previousPlayIndex = index + 1;

                return;
            }
            
            if (index == 0)
            {
                return;
            }

            PlayList.RemoveAt(index);
            PlayList.Insert(index - 1, item);

            _previousPlayIndex = index - 1;
        }
        private void MoveItemDown()
            => MoveItem(true);

        private void DockToggle()
            => IsDocked = !IsDocked;

        public void Update()
        {
            OnPropertyChanged(nameof(IsDocked), 
                nameof(IsShowing));
        }

        public bool FileExistsInList(string path)
            => PlayList.Any(x => x.FullFilePathName.Equals(path));

        public void SetNextSelectedToFile(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            var model = PlayList.FirstOrDefault(x => x.FullFilePathName.Equals(path));
            _previousPlayIndex = PlayList.IndexOf(model) - 1;

            HandleIsSelected(model);
        }

        #endregion

        #region Helpers

        public string? GetCurrentSelected()
            => PlayList?.FirstOrDefault(x => x.IsSelected)?.FullFilePathName;

        public string GetCurrentlyLoadedPlaylistFilePath()
            => _currentPlaylist;

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
