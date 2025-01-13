using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.Addressables
{
    public class AssetManager : IServiceAssetManager
    {
        private readonly IServiceImageLoader m_imageLoader;
        private readonly IServiceAudioLoader m_audioLoader;
        private readonly Dictionary<int, TextureData> _idToTexture;
        private readonly Dictionary<string, int> _pathToId;
        private int _nextId;
        private readonly Dictionary<int, IntPtr> _idToSound;
        private readonly Dictionary<string, int> _pathToSoundId;
        private int _nextSoundId;
        public AssetManager(IServiceImageLoader imageLoader, IServiceAudioLoader audioLoader)
        {
            _idToTexture = new Dictionary<int, TextureData>();
            _pathToId = new Dictionary<string, int>();
            _nextId = 1;

            _idToSound = new Dictionary<int, IntPtr>();
            _pathToSoundId = new Dictionary<string, int>();
            _nextSoundId = 1;

            m_imageLoader = imageLoader;
            m_audioLoader = audioLoader;
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

        /// <summary>
        /// Load and store a texture in hashmap
        /// </summary>
        /// <param name="renderer">SDL Renderer</param>
        /// <param name="path">Texture file path</param>
        /// <returns>TextureData object</returns>
        public TextureData LoadTexture(IntPtr renderer, string path)
        {
            if (_pathToId.ContainsKey(path))
            {
                int existingId = _pathToId[path];
                Debug.LogWarning($"Texture already loaded {path} with ID {existingId} </color>");
                return _idToTexture[existingId];
            }

            IntPtr surface = m_imageLoader.LoadImage(path);
            if (surface == IntPtr.Zero)
            {
                Debug.Throw<ArgumentNullException>(new ArgumentNullException(), $"Failed to load image: {path}");
                return null;
            }

            IntPtr texture = SDL.SDL_CreateTextureFromSurface(renderer, surface);
            SDL.SDL_FreeSurface(surface);

            if (texture == IntPtr.Zero)
                Debug.Throw<ArgumentNullException>(new ArgumentNullException(), $"Failed to create texture: {SDL.SDL_GetError()}");

            int id = _nextId++;

            SDL.SDL_QueryTexture(texture, out _, out _, out int width, out int height);
            SDL.SDL_Rect srcRect = new SDL.SDL_Rect { x = 0, y = 0, w = width, h = height };

            var textureData = new TextureData(id, texture, width, height, srcRect);
            _idToTexture[id] = textureData;
            _pathToId[path] = id;

            Debug.Log($"<color=green>Texture Created:</color> Id:{textureData.Id} Size:{textureData.Width}x{textureData.Height} Path:{path}");
            return textureData;
        }

        /// <summary>
        /// Draw a texture to a destination by ID
        /// </summary>
        /// <param name="renderer">SDL Renderer</param>
        /// <param name="textureId">Unique texture ID</param>
        /// <param name="dstRect">Destination rectangle</param>
        public void DrawTexture(IntPtr renderer, int textureId, ref SDL.SDL_Rect dstRect)
        {
            if (!_idToTexture.TryGetValue(textureId, out var textureData))
            {
                Debug.LogError($"Texture ID {textureId} not found.");
                return;
            }

            SDL.SDL_Rect srcRect = textureData.SrcRect;
            SDL.SDL_RenderCopy(renderer, textureData.Texture, ref srcRect, ref dstRect);
        }
        
        public void DrawTexture(IntPtr renderer, int textureId, ref SDL.SDL_Rect dstRect, ICamera camera)
        {
            if (!_idToTexture.TryGetValue(textureId, out var textureData))
            {
                Debug.LogError($"Texture ID {textureId} not found.");
                return;
            }

            if (camera == null)
            {
                Debug.LogError("Camera provided is null.");
                return;
            }

            SDL.SDL_Rect transformedDstRect = ApplyCameraTransform(dstRect, camera);
            SDL.SDL_Rect srcRect = textureData.SrcRect;
            SDL.SDL_RenderCopy(renderer, textureData.Texture, ref srcRect, ref transformedDstRect);
        }
        public void DrawTextureWithRotation(nint renderer, int textureId, ref SDL.SDL_Rect destRect, float rotation,
            ref SDL.SDL_Point center, ICamera camera)
        {
            if (!_idToTexture.TryGetValue(textureId, out var textureData))
            {
                Debug.LogError($"Texture ID {textureId} not found.");
                return;
            }

            if (camera == null)
            {
                Debug.LogError("Camera provided is null.");
                return;
            }
            
            float angleInDegrees = rotation * (180f / (float)Math.PI);

            SDL.SDL_Rect transformedDstRect = ApplyCameraTransform(destRect, camera);
            var srcRec = textureData.SrcRect;
            SDL.SDL_RenderCopyEx(renderer, textureData.Texture, ref srcRec, ref transformedDstRect, angleInDegrees, ref center,
                SDL.SDL_RendererFlip.SDL_FLIP_NONE);
        }


        public void DrawTextureWithRotation(nint renderer, int textureId, ref SDL.SDL_Rect destRect, float rotation,
            ref SDL.SDL_Point center)
        {
            if (!_idToTexture.TryGetValue(textureId, out var textureData))
            {
                Debug.LogError($"Texture ID {textureId} not found.");
                return;
            }

            float angleInDegrees = rotation * (180f / (float)Math.PI);

            var srcRec = textureData.SrcRect;
            SDL.SDL_RenderCopyEx(renderer, textureData.Texture, ref srcRec, ref destRect, angleInDegrees, ref center,
                SDL.SDL_RendererFlip.SDL_FLIP_NONE);
        }


        /// <summary>
        /// Unload a specific texture from asset manager
        /// </summary>
        /// <param name="id">Unique texture ID</param>
        public void UnloadTexture(int id)
        {
            if (_idToTexture.ContainsKey(id))
            {
                SDL.SDL_DestroyTexture(_idToTexture[id].Texture);
                _idToTexture.Remove(id);

                string path = _pathToId.FirstOrDefault(x => x.Value == id).Key;
                if (path != null)
                    _pathToId.Remove(path);

                Debug.Log($"Texture Unloaded: ID={id}, Path={path}");
            }
            else
            {
                Debug.Log($"<color=orange>WARNING: Attempted to unload non-existent texture ID: {id}</color>");
            }
        }

        /// <summary>
        /// Unloads ALL textures and sounds from asset manager
        /// </summary>
        public void Cleanup()
        {
            foreach (var textureData in _idToTexture.Values)
            {
                SDL.SDL_DestroyTexture(textureData.Texture);
            }
            
            foreach (var sound in _idToSound.Values)
            {
                SDL_mixer.Mix_FreeChunk(sound);
            }
            
            _idToTexture.Clear();
            _pathToId.Clear();
            _idToSound.Clear();
            _pathToSoundId.Clear();

            SDL_mixer.Mix_CloseAudio();
            SDL_image.IMG_Quit();

            Debug.Log("AssetManager Cleanup Completed.");
        }
        
        /// <summary>
        /// Applies camera transformation to the destination rectangle.
        /// </summary>
        /// <param name="dstRect">Original destination rectangle in world coordinates.</param>
        /// <param name="camera">Camera to apply transformation.</param>
        /// <returns>Transformed destination rectangle in screen coordinates.</returns>
        private SDL.SDL_Rect ApplyCameraTransform(SDL.SDL_Rect dstRect, ICamera camera)
        {
            // Calculate the offset based on camera position and zoom
            Vector2 cameraOffset = camera.GetOffset();

            // Apply zoom to the position and size
            float zoom = camera.Zoom;

            float transformedX = (dstRect.x - cameraOffset.X) * zoom;
            float transformedY = (dstRect.y - cameraOffset.Y) * zoom;
            float transformedW = dstRect.w * zoom;
            float transformedH = dstRect.h * zoom;

            // Create a new transformed SDL_Rect
            SDL.SDL_Rect transformedRect = new SDL.SDL_Rect
            {
                x = (int)transformedX,
                y = (int)transformedY,
                w = (int)transformedW,
                h = (int)transformedH
            };

            return transformedRect;
        }

        public class TextureData
        {
            public int Id { get; }
            public IntPtr Texture { get; }
            public int Width { get; }
            public int Height { get; }
            public SDL.SDL_Rect SrcRect { get; private set; }

            public TextureData(int id, IntPtr texture, int width, int height, SDL.SDL_Rect srcRect)
            {
                Id = id;
                Texture = texture;
                Width = width;
                Height = height;
                SrcRect = srcRect;
            }
        }
    }
}
