using static SDL2Engine.Core.Addressables.AudioLoader;

namespace SDL2Engine.Core.Addressables.Interfaces
{
    public interface IServiceAudioLoader
    {
        IntPtr LoadAudio(string path, AudioType audioType = AudioType.Wave);
        void CleanUp();
    }
}
