using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Cameras.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Addressables;

/// <summary>
/// DONT USE!
/// THIS GARBAGE NEEDS TO BE REDONE
/// </summary>
public class AudioService : IAudioService
{
    private readonly IAudioLoader m_audioLoader;
    private readonly Dictionary<nint, nint> _idToSound;
    private readonly Dictionary<string, nint> _pathToSoundId;
    private int _nextSoundId;

    public IReadOnlyDictionary<string, AudioLoader.FrequencyBand> FrequencyBands => m_audioLoader.FrequencyBands;

    public AudioService()
    {
        _idToSound = new Dictionary<nint, nint>();
        _pathToSoundId = new Dictionary<string, nint>();
        _nextSoundId = 1;

        m_audioLoader = new AudioLoader();
    }

    /// <summary>
    /// Load and return a sound pointer. If the sound is already loaded, return the existing pointer.
    /// </summary>
    /// <param name="path">File path to sound e.g., resources/sound/song.wav</param>
    /// <param name="audioType">Type of audio (default is Wave)</param>
    /// <returns>Unique sound ID</returns>
    public nint LoadSound(string path, AudioType audioType = AudioType.Wave)
    {
        if (_pathToSoundId.TryGetValue(path, out nint existingSoundId))
        {
            Debug.Log($"Sound already loaded: {path} with ID {existingSoundId}");
            return existingSoundId;
        }

        IntPtr sound = m_audioLoader.LoadAudio(path, audioType);
        if (sound == IntPtr.Zero)
        {
            Debug.LogError($"Failed to load sound: {path}");
            return -1;
        }

        int soundId = _nextSoundId++;
        _idToSound[soundId] = sound;
        _pathToSoundId[path] = soundId;
        return soundId;
    }

    /// <summary>
    /// Plays a sound effect on an available channel
    /// </summary>
    /// <param name="soundEffect"></param>
    /// <param name="volume"></param>
    /// <returns>sound channel</returns>
    public nint PlaySound(nint soundId, int volume = 128)
    {
        if (!_idToSound.TryGetValue(soundId, out IntPtr soundEffect))
        {
            Debug.LogError($"Sound ID {soundId} not found.");
            return 0;
        }

        try
        {
            var soundPtr = m_audioLoader.PlaySoundEffect(soundEffect, volume: volume);
            return soundPtr;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

        return 0;
    }

    public void PlayMusic(nint soundId, int volume = 128)
    { 
        m_audioLoader.PlayMusic(soundId, 0, volume: volume);
    }

    /// <summary>
    /// Unload a sound by its unique sound ID.
    /// </summary>
    /// <param name="soundId">Unique sound ID</param>
    public void UnloadSound(nint soundId)
    {
        if (_idToSound.TryGetValue(soundId, out nint sound))
        {
            SDL_mixer.Mix_FreeChunk(sound);
            _idToSound.Remove(soundId);

            string path = _pathToSoundId.FirstOrDefault(x => x.Value == soundId).Key;
            if (path != null)
                _pathToSoundId.Remove(path);

            Debug.Log($"Sound Unloaded: ID={soundId}, Path={path}");
        }
        else
        {
            Debug.LogWarning($"Attempted to unload non-existent sound ID: {soundId}");
        }
    }
    public void StopMusic()
    {
        SDL_mixer.Mix_HaltMusic();
    }

    public void StopSoundEffect(int channel)
    {
        SDL_mixer.Mix_HaltChannel(channel);
    }

    public void FreeSoundEffect(nint soundEffect)
    {
        SDL_mixer.Mix_FreeChunk(soundEffect);
    }

    public void FreeMusic(nint music)
    {
        SDL_mixer.Mix_FreeMusic(music);
    }

    public void UnregisterEffects(int channel)
    {
        SDL_mixer.Mix_UnregisterAllEffects(channel);
    }
    public float GetAmplitudeByName(string name)
    {
        return m_audioLoader.GetAmplitudeByName(name);
    }

    /// <summary>
    /// Unloads ALL sounds from asset manager
    /// </summary>
    public void Cleanup()
    {
        foreach (var sound in _idToSound.Values)
        {
            SDL_mixer.Mix_FreeChunk(sound);
        }

        _idToSound.Clear();
        _pathToSoundId.Clear();

        SDL_mixer.Mix_CloseAudio();

        Debug.Log("AudioService Cleanup Completed.");
    }
}
