using static SDL2Engine.Core.Addressables.AudioLoader;

namespace SDL2Engine.Core.Addressables.Interfaces
{
    public interface IAudioLoader
    {
        IReadOnlyDictionary<string, FrequencyBand> FrequencyBands { get; }
        nint LoadAudio(string path, AudioType audioType = AudioType.Wave);
        nint PlaySoundEffect(nint soundEffect, int channel = -1, int loops = 0, int volume = 128);
        void PlayMusic(nint music, int loops = -1, int volume = 128);
        // float GetAmplitudeByType(FreqBandType freqType);
        float GetAmplitudeByName(string name);
    }
}
