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
using System.IO;

namespace DigitalAudioExperiment.Logic
{
    public static class AudioPlayerFactory
    {
        public static IAudioPlayer GetAudioPlayerInterface(string fileName)
        {
            if (!File.Exists(fileName))
            {
                throw new FileNotFoundException();
            }

            var extension = Path.GetExtension(fileName).ToLower();
            IAudioPlayer player = null;
            switch (extension)
            {
                case ".mp3":
                    {
                        player = new AudioPlayerMp3(fileName);
                        break;
                    }
                case ".flac":
                    {
                        player = new AudioPlayerFlac(fileName);
                        break;
                    }
                case ".wav":
                    player = new AudioPlayerPcm(fileName);
                    break;
                default:
                    player = new AudioPlayerFallback(fileName);
                    break;
            }

            var retry = false;

            try
            {
                player.Initialise();
            }
            catch
            {
                if (player != null)
                {
                    player = new AudioPlayerFallback(fileName);

                    retry = true;
                }
            }

            if (retry
                && player != null)
            {
                try
                {
                    player.Initialise(); // Try again as might be valid fallback player.
                }
                catch (Exception exception)
                {
                    // We cant play this file even in fallback mode.

                    player?.Dispose();

                    player = null;

                    throw new ApplicationException($"No suitable player found for audio file extension {extension}", exception);
                }
            }

            return player;
        }
    }
}
