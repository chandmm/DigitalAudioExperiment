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
using DigitalAudioExperiment.Model;
using System.IO;

namespace DigitalAudioExperiment.ViewModel
{
    public class PlaylistPageViewModel : BaseViewModel
    {
        #region Fields

        private bool _isDisposed;
        private List<PlaylistModel> _playlist = new List<PlaylistModel>();

        #endregion

        #region Properties

        public bool IsShowing { get; set; }

        #endregion

        #region Manage List

        public void Add(string playlistItem)
            => _playlist.Add(new PlaylistModel() { FullFilePathName = playlistItem, FileName = Path.GetFileName(playlistItem)});

        public void Remove(string playlistItem)
        {
            var item = _playlist.FirstOrDefault(x => x.FullFilePathName == playlistItem);

            if (item != null)
            {
                _playlist.Remove(item);
            }
        }

        public void Add(PlaylistModel playlistModelItem)
            => _playlist.Add(playlistModelItem);

        public void Remove(PlaylistModel playlistItem)
            => _playlist.Remove(playlistItem);

        #endregion

        #region Dispose

        protected override void Dispose(bool isDisposng)
        {
            if (!_isDisposed)
            {
                if (isDisposng)
                {

                }

                _isDisposed = true;
            }
        }

        #endregion
    }
}
