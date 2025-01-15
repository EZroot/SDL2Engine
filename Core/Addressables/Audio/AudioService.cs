using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Addressables;

public class AudioService : IAudioService
{
    private readonly IAudioLoader m_audioLoader;
    private readonly Dictionary<int, IntPtr> _idToSound;
    private readonly Dictionary<string, int> _pathToSoundId;
    private int _nextSoundId;

    public IReadOnlyDictionary<string, AudioLoader.FrequencyBand> FrequencyBands => m_audioLoader.FrequencyBands;

    public AudioService()
    {
        _idToSound = new Dictionary<int, IntPtr>();
        _pathToSoundId = new Dictionary<string, int>();
        _nextSoundId = 1;

        m_audioLoader = new AudioLoader();
    }

    /// <summary>
    /// Load and return a sound pointer. If the sound is already loaded, return the existing pointer.
    /// </summary>
    /// <param name="path">File path to sound e.g., resources/sound/song.wav</param>
    /// <param name="audioType">Type of audio (default is Wave)</param>
    /// <returns>Unique sound ID</returns>
    public int LoadSound(string path, AudioType audioType = AudioType.Wave)
    {
        if (_pathToSoundId.TryGetValue(path, out int existingSoundId))
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
    /// Play a sound by its unique sound ID.
    /// </summary>
    /// <param name="soundId">Unique sound ID</param>
    /// <param name="volume">Volume level (0-128)</param>
    /// <param name="isMusic">Whether the sound is music</param>
    public void PlaySound(int soundId, int volume = 128, bool isMusic = false)
    {
        if (!_idToSound.TryGetValue(soundId, out IntPtr soundEffect))
        {
            Debug.LogError($"Sound ID {soundId} not found.");
            return;
        }

        if (isMusic)
            m_audioLoader.PlayMusic(soundEffect, volume: volume);
        else
            m_audioLoader.PlaySoundEffect(soundEffect, volume: volume);
    }

    /// <summary>
    /// Unload a sound by its unique sound ID.
    /// </summary>
    /// <param name="soundId">Unique sound ID</param>
    public void UnloadSound(int soundId)
    {
        if (_idToSound.TryGetValue(soundId, out IntPtr sound))
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
