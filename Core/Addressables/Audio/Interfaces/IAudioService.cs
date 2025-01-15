namespace SDL2Engine.Core.Addressables.Interfaces;

public interface IAudioService
{
    IReadOnlyDictionary<string, AudioLoader.FrequencyBand> FrequencyBands { get; }

    int LoadSound(string path, AudioType audioType = AudioType.Wave);
    void PlaySound(int soundId, int volume = 128, bool isMusic = false);
    void UnloadSound(int soundId);
    float GetAmplitudeByName(string name);
    void Cleanup();
}