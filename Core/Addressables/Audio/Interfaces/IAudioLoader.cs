using static SDL2Engine.Core.Addressables.AudioLoader;

namespace SDL2Engine.Core.Addressables.Interfaces
{
    public interface IAudioLoader
    {
        IReadOnlyDictionary<string, FrequencyBand> FrequencyBands { get; }
        IntPtr LoadAudio(string path, AudioType audioType = AudioType.Wave);
        void PlaySoundEffect(IntPtr soundEffect, int channel = -1, int loops = 0, int volume = 128);
        void PlayMusic(IntPtr music, int loops = -1, int volume = 128);
        // float GetAmplitudeByType(FreqBandType freqType);
        float GetAmplitudeByName(string name);
    }
}
