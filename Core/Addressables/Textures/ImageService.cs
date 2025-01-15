using System.Numerics;
using SDL2;
using SDL2Engine.Core.Addressables.Data;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Addressables;

public class ImageService : IImageService
{
        private readonly IImageLoader m_imageLoader;
        private readonly Dictionary<int, TextureData> _idToTexture;
        private readonly Dictionary<string, int> _pathToId;
        private int _nextId;
        public ImageService()
        {
            _idToTexture = new Dictionary<int, TextureData>();
            _pathToId = new Dictionary<string, int>();
            _nextId = 1;

            m_imageLoader = new ImageLoader();
        }

        public IntPtr LoadImageRaw(string path)
        {
            return m_imageLoader.LoadImage(path);
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
            
            _idToTexture.Clear();
            _pathToId.Clear();

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


}