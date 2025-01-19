using System;
using SDL2;
using SDL2Engine.Core.Addressables.Fonts.Interfaces;
using SDL2Engine.Core.Rendering.Interfaces;
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
                SDL_ttf.TTF_Quit();
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
                SDL_ttf.TTF_CloseFont(font);
                SDL_ttf.TTF_Quit();
                return new FontTexture(IntPtr.Zero, new SDL.SDL_Rect());
            }

            // Create texture from surface
            IntPtr textTexture = SDL.SDL_CreateTextureFromSurface(m_renderService.RenderPtr, textSurface);
            if (textTexture == IntPtr.Zero)
            {
                Debug.LogError($"Unable to create texture from rendered text! SDL_Error: {SDL.SDL_GetError()}");
                SDL.SDL_FreeSurface(textSurface);
                SDL_ttf.TTF_CloseFont(font);
                SDL_ttf.TTF_Quit();
                SDL.SDL_Quit();
                return new FontTexture(IntPtr.Zero, new SDL.SDL_Rect());
            }

            // Get text dimensions
            SDL.SDL_QueryTexture(textTexture, out _, out _, out int textWidth, out int textHeight);
            SDL.SDL_FreeSurface(textSurface); // Free the surface after creating the texture

            // Apply scaling
            int scaledWidth = (int)(textWidth * scale);
            int scaledHeight = (int)(textHeight * scale);

            // Define destination rectangle for rendering the text at the specified position
            SDL.SDL_Rect renderQuad = new SDL.SDL_Rect
            {
                x = position.x,
                y = position.y,
                w = scaledWidth,
                h = scaledHeight
            };

            // If scaling is applied, use SDL_RenderCopyEx for rotation or flipping if needed
            // Currently, SDL_RenderCopy is sufficient for scaling via the destination rectangle

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

        // Implement IDisposable for proper resource management
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
                    // Dispose managed resources here if any
                }

                // Dispose unmanaged resources
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
