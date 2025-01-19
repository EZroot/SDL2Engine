using SDL2;

namespace SDL2Engine.Core.Addressables.Fonts;

public struct FontTexture
{
    public IntPtr Texture { get; set; }
    public SDL.SDL_Rect RenderQuad { get; set; }

    public FontTexture(IntPtr texture, SDL.SDL_Rect renderQuad)
    {
        Texture = texture;
        RenderQuad = renderQuad;
    }
}