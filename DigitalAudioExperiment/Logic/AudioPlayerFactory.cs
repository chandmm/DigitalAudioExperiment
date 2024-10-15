using System.IO;

namespace DigitalAudioExperiment.Logic
{
    public static class AudioPlayerFactory
    {
        public static IAudioPlayer GetAudioPlayerInterface(string fileName)
        {
            var extension = Path.GetExtension(fileName).ToLower();
            IAudioPlayer player = null;

            switch (extension)
            {
                case ".mp3":
                    {
                        player = new AudioPlayerMp3(fileName);
                        break;
                    }
            }

            if (player == null)
            {
                throw new ApplicationException($"No suitable player found for audio file extension {extension}");
            }

            player.Initialise();

            return player;
        }
    }
}
