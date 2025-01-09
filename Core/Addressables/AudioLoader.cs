using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;
namespace SDL2Engine.Core.Addressables
{
    public class AudioLoader : IServiceAudioLoader
    {
        public enum AudioType 
        {
            Wave,
            Music
        }

        public AudioLoader()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (SDL_mixer.Mix_OpenAudio(44100, SDL.AUDIO_S16SYS, 2, 2048) < 0)
            {
                Debug.LogError("SDL_mixer could not initialize! SDL_mixer Error: " + SDL_mixer.Mix_GetError());
                return;
            }
            SDL_mixer.Mix_AllocateChannels(16);
            Debug.Log("<color=green> Successfully initialized MIX_OPENAUDIO() with (16) Channels</color>");
        }

        public IntPtr LoadAudio(string path, AudioType audioType = AudioType.Wave)
        {
            IntPtr soundEffect = IntPtr.Zero;
            switch(audioType)
            {
                case AudioType.Wave:
                    soundEffect = SDL_mixer.Mix_LoadWAV(path);
                break;
                case AudioType.Music:
                    soundEffect = SDL_mixer.Mix_LoadMUS(path);
                break;
            }

            if (soundEffect == IntPtr.Zero)
            {
                Debug.LogError("Failed to load sound effect! SDL_mixer Error: " + SDL_mixer.Mix_GetError());
                return IntPtr.Zero;
            }

            Debug.Log("<color=green>Raw Audio Loaded:</color> " + path);
            return soundEffect;
        }

        public void PlaySoundEffect(IntPtr soundEffect, int channel = -1, int loops = 0, int volume = 128)
        {
            SDL_mixer.Mix_VolumeChunk(soundEffect, volume);
            SDL_mixer.Mix_PlayChannel(channel, soundEffect, loops);
        }

        /// <summary>
        /// Play a music file. (Wave files specifically can't be played this way!)
        /// </summary>
        /// <param name="music"></param>
        /// <param name="loops"></param>
        /// <param name="volume"></param>
        public void PlayMusic(IntPtr music, int loops = -1, int volume = 128)
        {
            SDL_mixer.Mix_VolumeMusic(volume);
            SDL_mixer.Mix_PlayMusic(music, loops);
        }

        public void CleanUp()
        {
            SDL_mixer.Mix_CloseAudio();
        }
    }
}