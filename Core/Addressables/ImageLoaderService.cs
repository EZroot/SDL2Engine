using SDL2;
using SDL2Engine.Core.Addressables.Interfaces;
using SDL2Engine.Core.Utils;
namespace SDL2Engine.Core.Addressables
{
    public class ImageLoaderService : IImageLoaderService
    {
        public ImageLoaderService()
        {
            Initialize();
        }

        private void Initialize()
        {
            var imgFlags = SDL_image.IMG_InitFlags.IMG_INIT_PNG | SDL_image.IMG_InitFlags.IMG_INIT_JPG | SDL_image.IMG_InitFlags.IMG_INIT_TIF | SDL_image.IMG_InitFlags.IMG_INIT_WEBP;
            var imgInitFlag = SDL_image.IMG_Init(imgFlags);
            if (imgInitFlag != (int)imgFlags)
            {
                Debug.LogError($"ERROR: IMG_Init failed! {imgInitFlag} != {(int)imgFlags}");
                throw new Exception("IMG_INIT FAILURE");
            }
            else
            {
                Debug.Log("<color=green> Successfully initialized IMG_Init()</color>");
            }
        }

        public IntPtr LoadImage(string path)
        {
            IntPtr surface = SDL_image.IMG_Load(path);
            if (surface == IntPtr.Zero)
            {
                Debug.LogError("IMG_Load: Failed to load image! SDL_Error: " + SDL.SDL_GetError());
                return IntPtr.Zero;
            }
            Debug.Log("<color=green>Image Loaded:</color> " + path);

            return surface;
        }
    }
}