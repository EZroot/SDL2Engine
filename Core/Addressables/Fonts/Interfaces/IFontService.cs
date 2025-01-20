using SDL2;

namespace SDL2Engine.Core.Addressables.Fonts.Interfaces;

public interface IFontService
{
    /// <summary>
    /// Load a font and return the memory pointer
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    nint LoadFont(string path, int fontSize = 24);
    SpriteFontTexture LoadSpriteFont(string path, int frameWidth, int frameHeight, int charsPerRow = 26);
    FontTexture CreateFontTexture(IntPtr font, string message, SDL.SDL_Color color, (int x, int y) position,
        float scale = 1.0f);
    void RenderFontSpriteChar(SpriteFontTexture spriteFontTexture, char character, (int x, int y) position,
        float scale = 1.0f);

    void RenderStringSprite(SpriteFontTexture spriteFontTexture, string text, (int x, int y) position,
        int charPadding = 0, float scale = 1.0f);
    void RenderFont(FontTexture fontTexture);
    void CleanupFontTexture(FontTexture fontTexture);
}