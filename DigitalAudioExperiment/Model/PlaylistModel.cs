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
using DigitalAudioExperiment.Infrastructure;
using System.Xml.Serialization;

namespace DigitalAudioExperiment.Model
{
    public class PlaylistModel
    {
        public string FullFilePathName { get; set; }
        public string FileName { get; set; }
        public int SequenceId { get; private set; }
        public bool IsSelected { get; set; }

        public RelayCommand IsSelectedCommand { get; set; }

        public PlaylistModel(int maxId, Action<PlaylistModel> commandMethodCallback)
        {
            if (commandMethodCallback == null)
            {
                throw new ArgumentNullException(nameof(commandMethodCallback));
            }

            SequenceId = maxId + 1;
            
            IsSelectedCommand = new RelayCommand(() =>
            {
                commandMethodCallback(this);
            }, () => true);
        }

        internal void UpdateSequenceId(int i)
        {
            SequenceId = i;
        }
    }
}
