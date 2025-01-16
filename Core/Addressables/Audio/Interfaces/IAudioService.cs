namespace SDL2Engine.Core.Addressables.Interfaces;

public interface IAudioService
{
    IReadOnlyDictionary<string, AudioLoader.FrequencyBand> FrequencyBands { get; }

     nint LoadSound(string path, AudioType audioType = AudioType.Wave);
     nint PlaySound(nint soundId, int volume = 128);
     void PlayMusic(nint soundId, int volume = 128);

    /// <summary>
    /// Unload a sound by its unique sound ID.
    /// </summary>
    /// <param name="soundId">Unique sound ID</param>
     void UnloadSound(nint soundId);
     void StopMusic();

     void StopSoundEffect(int channel);

     void FreeSoundEffect(nint soundEffect);

     void FreeMusic(nint music);

     void UnregisterEffects(int channel);

     float GetAmplitudeByName(string name);
    /// <summary>
    /// Unloads ALL sounds from asset manager
    /// </summary>
     void Cleanup();
}