using System.Runtime.InteropServices;
using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;
using MathNet.Numerics.IntegralTransforms;
using System.Numerics;

namespace SDL2Engine.Core.Addressables
{
    public class AudioLoader : IServiceAudioLoader
    {
        private const int LOW_BAND_FREQ = 40;
        private const int MID_BAND_FREQ = 4000;
        public enum AudioType
        {
            Wave,
            Music
        }

        private SDL_mixer.Mix_EffectFunc_t _audioEffectDelegate;
        private SDL_mixer.Mix_EffectDone_t _effectDoneDelegate;

        public float PlayingSongLowFreqBand { get;  private set; }
        public float PlayingSongMidFreqBand { get;  private set; }
        public float PlayingSongHighFreqBand { get;  private set; }

        public AudioLoader()
        {
            _audioEffectDelegate = AudoProcessor;
            _effectDoneDelegate = AudoProcessorDone;
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
            switch (audioType)
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
            int playingChannel = SDL_mixer.Mix_PlayChannel(channel, soundEffect, loops);

            if (playingChannel != -1)
            {
                RegisterAudioEffect(playingChannel);
            }
            else
            {
                Debug.LogError("Failed to play sound effect!");
            }
        }

        public void PlayMusic(IntPtr music, int loops = -1, int volume = 128)
        {
            SDL_mixer.Mix_VolumeMusic(volume);
            SDL_mixer.Mix_PlayMusic(music, loops);
        }

        public void RegisterAudioEffect(int channel)
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

        private void AudoProcessor(int channel, IntPtr stream, int length, IntPtr userData)
        {
            byte[] audioBytes = new byte[length];
            Marshal.Copy(stream, audioBytes, 0, length);

            int sampleCount = length / 2;
            Complex[] fftBuffer = new Complex[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                short sample = BitConverter.ToInt16(audioBytes, i * 2);
                fftBuffer[i] = new Complex(sample / 32768.0, 0);
            }

            Fourier.Forward(fftBuffer, FourierOptions.Matlab);

            double bassAmplitude = 0;
            double midAmplitude = 0;
            double highAmplitude = 0;

            int sampleRate = 44100;
            for (int i = 0; i < fftBuffer.Length / 2; i++)
            {
                double magnitude = fftBuffer[i].Magnitude;
                double frequency = i * sampleRate / fftBuffer.Length;

                if (frequency < LOW_BAND_FREQ)
                    bassAmplitude += magnitude; // Bass freq
                else if (frequency < MID_BAND_FREQ)
                    midAmplitude += magnitude; // Mid freq (vocals, melodies)
                else
                    highAmplitude += magnitude; // High freq (whatever else)
            }

            double totalAmplitude = bassAmplitude + midAmplitude + highAmplitude;
            if (totalAmplitude > 0)
            {
                bassAmplitude /= totalAmplitude;
                midAmplitude /= totalAmplitude;
                highAmplitude /= totalAmplitude;
            }

            // Debug.Log($"Total: {totalAmplitude} bass{bassAmplitude} mid{midAmplitude} high{highAmplitude}");
            PlayingSongLowFreqBand = (float)bassAmplitude;
            PlayingSongMidFreqBand = (float)midAmplitude;
            PlayingSongHighFreqBand = (float)highAmplitude;
        }


        private void AudoProcessorDone(int channel, IntPtr userData)
        {
        }

        public void CleanUp()
        {
            SDL_mixer.Mix_CloseAudio();
        }
    }
}
