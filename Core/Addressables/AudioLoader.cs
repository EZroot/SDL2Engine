using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;
namespace SDL2Engine.Core.Addressables
{
    public class AudioLoader : IServiceAudioLoader
    {
        public enum AudioType 
        {
            Wave
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
            else
            {
                Debug.Log("<color=green> Successfully initialized MIX_OPENAUDIO()</color>");
            }
        }

        public IntPtr LoadAudio(string path, AudioType audioType = AudioType.Wave)
        {
            IntPtr soundEffect = IntPtr.Zero;
            switch(audioType)
            {
                case AudioType.Wave:
                Debug.Log($"Trying to load wav {path}");
                    soundEffect = SDL_mixer.Mix_LoadWAV(path);
                break;
            }

            if (soundEffect == IntPtr.Zero)
            {
                Debug.LogError("Failed to load sound effect! SDL_mixer Error: " + SDL_mixer.Mix_GetError());
            }

            return soundEffect;
        }

        public void CleanUp()
        {
            SDL_image.IMG_Quit();
            SDL.SDL_Quit();
        }
    }
}