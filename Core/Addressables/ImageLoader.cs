using SDL2;
namespace SDL2Engine.Core.Addressables
{
public class ImageLoader : IServiceImageLoader
{
    public ImageLoader()
    {
        
    }

    public void Initialize()
    {
        var imgFlags = SDL_image.IMG_InitFlags.IMG_INIT_PNG | SDL_image.IMG_InitFlags.IMG_INIT_JPG | SDL_image.IMG_InitFlags.IMG_INIT_TIF | SDL_image.IMG_InitFlags.IMG_INIT_WEBP;
        var inittedFlags = SDL_image.IMG_Init(imgFlags);
        // if ((inittedFlags & imgFlags) != imgFlags)
        // {
        //     Console.WriteLine("IMG_Init: Failed to init required jpg and png support!");
        //     Console.WriteLine("IMG_Init: " + SDL.SDL_GetError());
        // }
    }

    public IntPtr LoadImage(string path)
    {
        IntPtr surface = SDL_image.IMG_Load(path);
        if (surface == IntPtr.Zero)
        {
            Console.WriteLine("IMG_Load: Failed to load image! SDL_Error: " + SDL.SDL_GetError());
        }
        return surface;
    }

    public void Shutdown()
    {
        SDL_image.IMG_Quit();
        SDL.SDL_Quit();
    }
}
}