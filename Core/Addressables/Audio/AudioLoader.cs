using System.Runtime.InteropServices;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;
using SDL2Engine.Events;
using SDL2Engine.Events.EventData.Audio;

namespace SDL2Engine.Core.Addressables
{
    public class AudioLoader : IAudioLoader
    {
        public class FrequencyBand
        {
            public double LowerBound { get; }
            public double UpperBound { get; }
            public float Amplitude { get; set; }

            public FrequencyBand(double lowerBound, double upperBound)
            {
                LowerBound = lowerBound;
                UpperBound = upperBound;
                Amplitude = 0f;
            }
        }

        private readonly Dictionary<string, FrequencyBand> _frequencyBands = new();

        private SDL_mixer.Mix_EffectFunc_t _audioEffectDelegate;
        private SDL_mixer.Mix_EffectDone_t _effectDoneDelegate;
        private SDL_mixer.MusicFinishedDelegate _musicFinishedDelegate; // Added delegate field
        
        public IReadOnlyDictionary<string, FrequencyBand> FrequencyBands => _frequencyBands;

        private readonly object _lock = new object();

        public AudioLoader()
        {
            var startFrequency = 0;
            var frequencyStep = 25;

            for (var i = 0; i < 32; i++)
            {
                var lowerBound = startFrequency + i * frequencyStep;
                var upperBound = lowerBound + frequencyStep - 1;

                FrequencyBand band = new FrequencyBand(lowerBound, upperBound); 
                _frequencyBands.Add(i.ToString(), band);
                if (i == 0) // Sub-bass
                {
                    frequencyStep = 25; 
                }
                else if (i == 1) // Bass
                {
                    frequencyStep = 50;
                }
                else if (i == 5) // Lower midrange (includes lower vocals)
                {
                    frequencyStep = 100;
                }
                else if (i == 10) // Higher midrange (includes main vocals)
                {
                    frequencyStep = 200;
                }
                else if (i == 15) // Presence range (includes higher range instruments)
                {
                    frequencyStep = 550;
                }
                else if (i == 19)
                {
                    frequencyStep = 700;
                }
                // else if (i == 20) // Brilliance range (highest audible frequencies)
                // {
                //     frequencyStep = 1000;
                // }
            }
            
            _audioEffectDelegate = AudioProcessor;
            _effectDoneDelegate = AudioProcessorDone;
            _musicFinishedDelegate = OnMusicFinished;
            
            SDL_mixer.Mix_HookMusicFinished(_musicFinishedDelegate);

            Initialize();
        }

        private void Initialize()
        {
            try
            {
                if (SDL_mixer.Mix_OpenAudio(44100, SDL.AUDIO_S16SYS, 2, 2048) < 0)
                {
                    Debug.LogError("SDL_mixer could not initialize! SDL_mixer Error: " + SDL_mixer.Mix_GetError());
                    return;
                }

                SDL_mixer.Mix_AllocateChannels(16);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            Debug.Log("<color=green> Successfully initialized MIX_OPENAUDIO() with (16) Channels</color>");
        }

        /// <summary>
        /// Loads an audio file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="audioType"></param>
        /// <returns>loaded sound pointer</returns>
        public nint LoadAudio(string path, AudioType audioType = AudioType.Wave)
        {

            nint soundEffect = IntPtr.Zero;
            try
            {
                switch (audioType)
                {
                    case AudioType.Wave:
                        soundEffect = SDL_mixer.Mix_LoadWAV(path);
                        break;
                    case AudioType.Music:
                        soundEffect = SDL_mixer.Mix_LoadMUS(path);
                        break;
                    default:
                        Debug.LogError("Unsupported AudioType: " + audioType);
                        break;
                }

                if (soundEffect == IntPtr.Zero)
                {
                    Debug.LogError("Failed to load sound effect! SDL_mixer Error: " + SDL_mixer.Mix_GetError());
                    return IntPtr.Zero;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }

            Debug.Log("<color=green>Raw Audio Loaded:</color> " + path);
            return soundEffect;
        }

        /// <summary>
        /// Plays a sound effect on an available channel
        /// </summary>
        /// <param name="soundEffect"></param>
        /// <param name="channel"></param>
        /// <param name="loops"></param>
        /// <param name="volume"></param>
        /// <returns>sound channel</returns>
        public nint PlaySoundEffect(IntPtr soundEffect, int channel = -1, int loops = 0, int volume = 128)
        {
            int playingChannel = 0;
            try
            {
                SDL_mixer.Mix_VolumeChunk(soundEffect, volume);
                playingChannel = SDL_mixer.Mix_PlayChannel(channel, soundEffect, loops);

                if (playingChannel != -1)
                {
                    RegisterAudioEffect(playingChannel);
                }
                else
                {
                    Debug.LogError("Failed to play sound effect! SDL_mixer Error: " + SDL_mixer.Mix_GetError());
                }
            }
            catch (Exception e)
            {
                
            }

            return playingChannel;
        }

        /// <summary>
        /// Plays music on a dedicated music channel
        /// </summary>
        /// <param name="music"></param>
        /// <param name="loops"></param>
        /// <param name="volume"></param>
        public void PlayMusic(nint music, int loops = -1, int volume = 128)
        {
            // try
            // {
                // SDL_mixer.Mix_VolumeMusic(volume);
                if (SDL_mixer.Mix_PlayMusic(music, loops) == -1)
                {
                    Debug.LogError("Failed to play music! SDL_mixer Error: " + SDL_mixer.Mix_GetError());
                }
            // }
            // catch (Exception e)
            // {
            //     Debug.LogError(e.Message);
            // }
        }

        public void RegisterAudioEffect(int channel)
        {
            try
            {
                int result = SDL_mixer.Mix_RegisterEffect(channel, _audioEffectDelegate, _effectDoneDelegate, IntPtr.Zero);
                if (result == 0)
                {
                    Debug.LogError("Failed to register audio effect! SDL_mixer Error: " + SDL_mixer.Mix_GetError());
                }
                else
                {
                    Debug.Log("Audio effect registered successfully!");
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        /// <summary>
        /// Retrieves the amplitude for a given frequency band type.
        /// </summary>
        /// <param name="freqType">The frequency band type.</param>
        /// <returns>The amplitude of the specified frequency band.</returns>
        // public float GetAmplitudeByType(FreqBandType freqType)
        // {
        //     string bandName = freqType.ToString();
        //     return GetAmplitudeByName(bandName);
        // }

        /// <summary>
        /// Retrieves the amplitude for a given frequency band name.
        /// </summary>
        /// <param name="name">The name of the frequency band.</param>
        /// <returns>The amplitude of the specified frequency band.</returns>
        public float GetAmplitudeByName(string name)
        {
            lock (_lock) 
            {
                if (_frequencyBands.TryGetValue(name, out var band))
                {
                    return band.Amplitude;
                }
            }
            Debug.Log($"Frequency band '{name}' not found.");
            return 0f;
        }

        private void AudioProcessor(int channel, IntPtr stream, int length, IntPtr userData)
        {
            try
            {
                byte[] audioBytes = new byte[length];
                Marshal.Copy(stream, audioBytes, 0, length);

                int sampleCount = length / 2; // 16-bit audio
                Complex[] fftBuffer = new Complex[sampleCount];

                for (int i = 0; i < sampleCount; i++)
                {
                    short sample = BitConverter.ToInt16(audioBytes, i * 2);
                    fftBuffer[i] = new Complex(sample / 32768.0, 0);
                }

                ApplyHannWindow(fftBuffer);
                Fourier.Forward(fftBuffer, FourierOptions.Matlab);

                double sampleRate = 44100.0;
                lock (_lock)
                {
                    foreach (var band in _frequencyBands.Values)
                    {
                        band.Amplitude = 0f;
                    }

                    double totalAmplitude = 0.0;

                    for (int i = 0; i < fftBuffer.Length / 2; i++)
                    {
                        double magnitude = fftBuffer[i].Magnitude;
                        double frequency = i * sampleRate / fftBuffer.Length;

                        foreach (var kvp in _frequencyBands)
                        {
                            var band = kvp.Value;
                            if (frequency >= band.LowerBound && frequency < band.UpperBound)
                            {
                                band.Amplitude += (float)magnitude;
                                break;
                            }
                        }

                        totalAmplitude += magnitude;
                    }

                    if (totalAmplitude > 0)
                    {
                        foreach (var band in _frequencyBands.Values)
                        {
                            band.Amplitude = (float)(band.Amplitude / totalAmplitude);
                        }
                    }

                    // string amplitudeLog = "Frequency Amplitudes: ";
                    // foreach (var kvp in _frequencyBands)
                    // {
                    //     amplitudeLog += $"{kvp.Key}: {kvp.Value.Amplitude:F2} ";
                    // }
                    // Debug.Log(amplitudeLog);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        private void AudioProcessorDone(int channel, IntPtr userData)
        {
            Debug.Log($"Audio processing done on channel {channel}.");
            EventHub.Raise(this, new OnAudioProcessFinished());
        }

        private void OnMusicFinished()
        {
            Debug.Log("Music finished playing.");
            EventHub.Raise(this, new OnMusicFinishedPlaying());
        }


        private void ApplyHannWindow(Complex[] buffer)
        {
            int N = buffer.Length;
            for (int n = 0; n < N; n++)
            {
                double hann = 0.5 * (1 - Math.Cos(2 * Math.PI * n / (N - 1)));
                buffer[n] *= hann;
            }
        }
    }
}
