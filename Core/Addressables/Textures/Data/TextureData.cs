using SDL2;

namespace SDL2Engine.Core.Addressables.Data;

public class TextureData
{
    public int Id { get; }
    public IntPtr Texture { get; }
    public int Width { get; }
    public int Height { get; }
    public SDL.SDL_Rect? SrcRect { get; private set; }

    public TextureData(int id, IntPtr texture, int width, int height, SDL.SDL_Rect? srcRect)
    {
        Id = id;
        Texture = texture;
        Width = width;
        Height = height;
        SrcRect = srcRect;
    }
}
