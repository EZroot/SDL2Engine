using System;
using SDL2;
using SDL2Engine.Core.Addressables.Fonts.Interfaces;
using SDL2Engine.Core.Cameras.Interfaces;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Addressables.Fonts
{
    public class FontService : IFontService, IDisposable
    {
        private readonly IRenderService m_renderService;
        private bool disposed = false;

        public FontService(IRenderService renderService)
        {
            m_renderService = renderService;
            if (SDL_ttf.TTF_Init() == -1)
            {
                Debug.LogError("Failed to initialize SDL Font Service!");
            }
        }

        /// <summary>
        /// Loads a sprite sheet font from the specified path.
        /// </summary>
        /// <param name="path">Path to the sprite sheet image file.</param>
        public SpriteFontTexture LoadSpriteFont(string path, int frameWidth, int frameHeight, int charsPerRow = 26)
        {
            var spriteSheetTexture = SDL_image.IMG_LoadTexture(m_renderService.RenderPtr, path);
            if (spriteSheetTexture == IntPtr.Zero)
            {
                Debug.LogError($"Failed to load sprite sheet! SDL_Error: {SDL.SDL_GetError()}");
            }

            var spriteFont = new SpriteFontTexture()
            {
                Texture = spriteSheetTexture,
                Width = frameWidth,
                Height = frameHeight,
                CharsPerRow = charsPerRow
            };

            return spriteFont;
        }
        
        /// <summary>
        /// Renders a character from the sprite sheet.
        /// </summary>
        /// <param name="character">Character to render.</param>
        /// <param name="position">Position to render the character (x, y).</param>
        /// <param name="scale">Scale factor for the character.</param>
        public void RenderFontSpriteChar(SpriteFontTexture spriteFontTexture, char character, (int x, int y) position, float scale = 1.0f)
        {
            int charIndex = GetCharIndex(character);
            int charsPerRow = spriteFontTexture.CharsPerRow; 
            int srcX = (charIndex % charsPerRow) * spriteFontTexture.Width;
            int srcY = (charIndex / charsPerRow) * spriteFontTexture.Height;

            SDL.SDL_Rect srcRect = new SDL.SDL_Rect { x = srcX, y = srcY, w = spriteFontTexture.Width, h = spriteFontTexture.Height };
            SDL.SDL_Rect destRect = new SDL.SDL_Rect
            {
                x = position.x,
                y = position.y,
                w = (int)(spriteFontTexture.Width * scale),
                h = (int)(spriteFontTexture.Height * scale)
            };

            SDL.SDL_RenderCopy(m_renderService.RenderPtr, spriteFontTexture.Texture, ref srcRect, ref destRect);
        }
        
        /// <summary>
        /// Renders a string using the sprite sheet, each character from the sprite sheet.
        /// </summary>
        /// <param name="text">String to render.</param>
        /// <param name="position">Starting position to render the string (x, y).</param>
        /// <param name="scale">Scale factor for each character.</param>
        public void RenderStringSprite(SpriteFontTexture spriteFontTexture, string text, (int x, int y) position, int charPadding = 0, float scale = 1.0f)
        {
            int charsPerRow = spriteFontTexture.CharsPerRow; 
            int characterWidth = spriteFontTexture.Width; 
            int characterHeight = spriteFontTexture.Height; 
            int spacing = (int)(characterWidth * scale) + charPadding; 

            for (int i = 0; i < text.Length; i++)
            {
                char character = text[i];
                int charIndex = GetCharIndex(character);
                if (charIndex == -1) continue; 

                int srcX = (charIndex % charsPerRow) * characterWidth;
                int srcY = (charIndex / charsPerRow) * characterHeight;

                SDL.SDL_Rect srcRect = new SDL.SDL_Rect { x = srcX, y = srcY, w = characterWidth, h = characterHeight };
                SDL.SDL_Rect destRect = new SDL.SDL_Rect
                {
                    x = position.x + (i * spacing), 
                    y = position.y,
                    w = (int)(characterWidth * scale),
                    h = (int)(characterHeight * scale)
                };

                SDL.SDL_RenderCopy(m_renderService.RenderPtr, spriteFontTexture.Texture, ref srcRect, ref destRect);
            }
        }
        
        /// <summary>
        /// Loads a font from the specified path with the given size.
        /// </summary>
        /// <param name="path">Path to the TTF font file.</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <returns>Pointer to the loaded font, or IntPtr.Zero on failure.</returns>
        public IntPtr LoadFont(string path, int fontSize = 24)
        {
            IntPtr font = SDL_ttf.TTF_OpenFont(path, fontSize);
            if (font == IntPtr.Zero)
            {
                Debug.LogError($"Failed to load font! TTF_Error: {SDL_ttf.TTF_GetError()}");
                return IntPtr.Zero;
            }

            return font;
        }

        /// <summary>
        /// Creates a font texture with specified position and scale.
        /// </summary>
        /// <param name="font">Loaded font pointer.</param>
        /// <param name="message">Text to render.</param>
        /// <param name="color">Color of the text.</param>
        /// <param name="position">Position to render the text (x, y).</param>
        /// <param name="scale">Scale factor for the text.</param>
        /// <returns>A FontTexture containing the texture and its render rectangle.</returns>
        public FontTexture CreateFontTexture(IntPtr font, string message, SDL.SDL_Color color, (int x, int y) position, float scale = 1.0f)
        {
            // Render text to surface
            IntPtr textSurface = SDL_ttf.TTF_RenderText_Blended(font, message, color);
            if (textSurface == IntPtr.Zero)
            {
                Debug.LogError($"Unable to render text surface! TTF_Error: {SDL_ttf.TTF_GetError()}");
                return new FontTexture(IntPtr.Zero, new SDL.SDL_Rect());
            }

            // Create texture from surface
            IntPtr textTexture = SDL.SDL_CreateTextureFromSurface(m_renderService.RenderPtr, textSurface);
            if (textTexture == IntPtr.Zero)
            {
                Debug.LogError($"Unable to create texture from rendered text! SDL_Error: {SDL.SDL_GetError()}");
                return new FontTexture(IntPtr.Zero, new SDL.SDL_Rect());
            }

            SDL.SDL_QueryTexture(textTexture, out _, out _, out int textWidth, out int textHeight);
            SDL.SDL_FreeSurface(textSurface);

            int scaledWidth = (int)(textWidth * scale);
            int scaledHeight = (int)(textHeight * scale);

            SDL.SDL_Rect renderQuad = new SDL.SDL_Rect
            {
                x = position.x,
                y = position.y,
                w = scaledWidth,
                h = scaledHeight
            };
            return new FontTexture(textTexture, renderQuad);
        }

        /// <summary>
        /// Renders the font texture at its designated position and scale.
        /// </summary>
        /// <param name="fontTexture">The FontTexture to render.</param>
        public void RenderFont(FontTexture fontTexture)
        {
            if (fontTexture.Texture != IntPtr.Zero)
            {
                var renderQuad = fontTexture.RenderQuad;
                // use SDL_RenderCopyEx for rotation or flipping if needed
                SDL.SDL_RenderCopy(m_renderService.RenderPtr, fontTexture.Texture, IntPtr.Zero, ref renderQuad);
            }
        }

        /// <summary>
        /// Cleans up resources associated with a font texture.
        /// </summary>
        /// <param name="fontTexture">The FontTexture to clean up.</param>
        public void CleanupFontTexture(FontTexture fontTexture)
        {
            if (fontTexture.Texture != IntPtr.Zero)
            {
                SDL.SDL_DestroyTexture(fontTexture.Texture);
            }
        }
        
        /// <summary>
        /// Assuming the layout is: ABC...XYZ0123456789!.,?><=+-
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private int GetCharIndex(char character)
        {
            if (char.IsLetter(character))
            {
                return char.ToUpper(character) - 'A';
            }
            else if (char.IsDigit(character))
            {
                return 26 + (character - '0'); 
            }
            else
            {
                // Assuming the layout is: ABC...XYZ0123456789!.,?><=+-
                switch (character)
                {
                    case '!': return 36; 
                    case '.': return 37;
                    case ',': return 38;
                    case '?': return 39;
                    case '>': return 40;
                    case '<': return 41;
                    case '=': return 42;
                    case '+': return 43;
                    case '-': return 44;
                    default: return -1; 
                }
            }
            return -1;
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {

                }

                SDL_ttf.TTF_Quit();

                disposed = true;
            }
        }

        ~FontService()
        {
            Dispose(false);
        }
    }
}
