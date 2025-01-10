using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Addressables
{
    public class AssetManager : IServiceAssetManager
    {
        private readonly IServiceImageLoader m_imageLoader;
        private readonly IServiceAudioLoader m_audioLoader;
        private readonly Dictionary<int, TextureData> _idToTexture;
        private readonly Dictionary<string, int> _pathToId;
        private int _nextId;

        public AssetManager(IServiceImageLoader imageLoader, IServiceAudioLoader audioLoader)
        {
            _idToTexture = new Dictionary<int, TextureData>();
            _pathToId = new Dictionary<string, int>();
            _nextId = 1;

            m_imageLoader = imageLoader;
            m_audioLoader = audioLoader;
        }
        
        /// <summary>
        /// Load and return a sound pointer
        /// </summary>
        /// <param name="path">File path to sound eg: resources/sound/song.wav</param>
        /// <param name="audioType"></param>
        /// <returns></returns>
        public IntPtr LoadSound(string path, AudioType audioType = AudioType.Wave)
        {
            return m_audioLoader.LoadAudio(path, audioType);
        }

        /// <summary>
        /// Play a given sound
        /// </summary>
        /// <param name="soundEffect"></param>
        public void PlaySound(IntPtr soundEffect, int volume = 128, bool isMusic = false)
        {
            if(isMusic)
                m_audioLoader.PlayMusic(soundEffect, volume: volume);
            else
                m_audioLoader.PlaySoundEffect(soundEffect, volume: volume);
        }

        public void UnloadSound(IntPtr soundEffect)
        {
            SDL_mixer.Mix_FreeChunk(soundEffect);
        }

        /// <summary>
        /// Load and store a texture in hashmap
        /// </summary>
        /// <param name="path"></param>
        /// <returns>TextureData</returns>
        public TextureData LoadTexture(IntPtr renderer, string path)
        {
            if (_pathToId.ContainsKey(path))
            {
                int existingId = _pathToId[path];
                Debug.Log($"WARNING: Texture already loaded {path}");
                return _idToTexture[existingId];
            }

            IntPtr surface = m_imageLoader.LoadImage(path);
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

            Debug.Log($"<color=green>Texture Created: </color> Id:{textureData.Id} Size:{textureData.Width}x{textureData.Height} Path:{path}");
            return textureData;
        }

        /// <summary>
        /// Draw a texture to a destination by ID
        /// </summary>
        /// <param name="textureId"></param>
        /// <param name="dstRect"></param>
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

        /// <summary>
        /// Unload a specific texture from asset manager
        /// </summary>
        /// <param name="id"></param>
        public void UnloadTexture(int id)
        {
            if (_idToTexture.ContainsKey(id))
            {
                SDL.SDL_DestroyTexture(_idToTexture[id].Texture);
                _idToTexture.Remove(id);

                string path = _pathToId.FirstOrDefault(x => x.Value == id).Key;
                if (path != null)
                    _pathToId.Remove(path);
            }
        }

        /// <summary>
        /// Unloads ALL textures from asset manager
        /// </summary>
        public void Cleanup()
        {
            foreach (var textureData in _idToTexture.Values)
            {
                SDL.SDL_DestroyTexture(textureData.Texture);
            }
            _idToTexture.Clear();
            _pathToId.Clear();

            SDL_mixer.Mix_CloseAudio();
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
