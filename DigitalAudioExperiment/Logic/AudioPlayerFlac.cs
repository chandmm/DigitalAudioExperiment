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
namespace DigitalAudioExperiment.Logic
{
    public class AudioPlayerFlac : AudioPlayerBase, IAudioPlayer
    {
        public AudioPlayerFlac(string fileName) 
            : base(fileName)
        {
        }

        public override void Initialise()
        {

        }

        public override string GetAudioFileInfo()
        {
            throw new NotImplementedException();
        }

        public override double GetElapsed()
        {
            throw new NotImplementedException();
        }

        public override int? GetFrameCount()
        {
            throw new NotImplementedException();
        }

        public override bool GetIsMonoChannel()
        {
            throw new NotImplementedException();
        }

        public override string GetMetadata()
        {
            throw new NotImplementedException();
        }

        public override void Initialise()
        {
            throw new NotImplementedException();
        }
    }
}
