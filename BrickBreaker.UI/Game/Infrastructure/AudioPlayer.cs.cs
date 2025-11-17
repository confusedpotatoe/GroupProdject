using NAudio.Wave;


namespace BrickBreaker.Game.Infrastructure
{
    public class AudioPlayer : IDisposable
    {
        private IWavePlayer? soundtrackPlayer;
        private AudioFileReader? soundtrackReader;
        private bool musicActive = false;
        private EventHandler<StoppedEventArgs>? playbackStoppedHandler;

        private readonly string[] playlist = new string[]
        {
            "Assets/Sounds/Backbeat.mp3",
            "Assets/Sounds/Arpent.mp3"
        };
        private int currentTrack = 0;

        public void StartMusic()
        {
            musicActive = true;
            soundtrackReader = new AudioFileReader(playlist[currentTrack]);
            soundtrackPlayer = new WaveOutEvent();

            playbackStoppedHandler = (s, e) =>
            {
                if (!musicActive) return;
                currentTrack = (currentTrack + 1) % playlist.Length;

                soundtrackReader?.Dispose();
                soundtrackReader = new AudioFileReader(playlist[currentTrack]);

                soundtrackPlayer?.Init(soundtrackReader);
                soundtrackPlayer?.Play();
            };

            soundtrackPlayer.PlaybackStopped += playbackStoppedHandler;
            soundtrackPlayer.Init(soundtrackReader);
            soundtrackPlayer.Play();
        }

        public void StopMusic()
        {
            musicActive = false;
            if (soundtrackPlayer != null)
            {
                if (playbackStoppedHandler != null)
                    soundtrackPlayer.PlaybackStopped -= playbackStoppedHandler;

                soundtrackPlayer.Stop();
                soundtrackPlayer.Dispose();
                soundtrackPlayer = null;
            }
            if (soundtrackReader != null)
            {
                soundtrackReader.Dispose();
                soundtrackReader = null;
            }
        }

        public void Dispose()
        {
            StopMusic();
            GC.SuppressFinalize(this);
        }
    }
}