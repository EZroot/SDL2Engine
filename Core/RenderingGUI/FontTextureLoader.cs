using ImGuiNET;
using SDL2;
namespace SDL2Engine.Core.GuiRenderer
{
    public class FontTextureLoader : IDisposable
    {
        private IntPtr m_rendererPtr;
        private IntPtr m_fontTexturePtr;
        private bool m_isDisposed;

        public FontTextureLoader(IntPtr renderer)
        {
            m_rendererPtr = renderer;
        }

        /// <summary>
        /// Loads the current font texture, or IMGui default font if none specified.
        /// </summary>
        /// <exception cref="Exception"></exception>
        public void LoadFontTextureSDL()
        {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out IntPtr pixelData, out int textureWidth, out int textureHeight, out int bytesPerPixel);

            if (pixelData == IntPtr.Zero)
            {
                throw new Exception("Failed to load font texture data.");
            }

            m_fontTexturePtr = SDL.SDL_CreateTexture(m_rendererPtr, SDL.SDL_PIXELFORMAT_ABGR8888, (int)SDL.SDL_TextureAccess.SDL_TEXTUREACCESS_STATIC, textureWidth, textureHeight);

            if (m_fontTexturePtr == IntPtr.Zero)
            {
                throw new Exception($"Failed to create SDL texture: {SDL.SDL_GetError()}");
            }

            SDL.SDL_UpdateTexture(m_fontTexturePtr, IntPtr.Zero, pixelData, textureWidth * bytesPerPixel);
            SDL.SDL_SetTextureBlendMode(m_fontTexturePtr, SDL.SDL_BlendMode.SDL_BLENDMODE_BLEND);

            io.Fonts.SetTexID(m_fontTexturePtr);
            io.Fonts.ClearTexData();
        }

        /// <summary>
        /// Load a font from file
        /// </summary>
        /// <param name="fontPath"></param>
        /// <param name="fontSize"></param>
        /// <exception cref="FileNotFoundException"></exception>
        public void LoadFontFromFileSDL(string fontPath, float fontSize)
        {
            if (!File.Exists(fontPath))
            {
                throw new FileNotFoundException("Font file not found.", fontPath);
            }

            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.AddFontFromFileTTF(fontPath, fontSize);
            io.Fonts.Build();
            
            // Re-invoke loading texture data after adding a new font
            LoadFontTextureSDL();
        }

        public void Dispose()
        {
            if (m_isDisposed) return;

            if (m_fontTexturePtr != IntPtr.Zero)
            {
                SDL.SDL_DestroyTexture(m_fontTexturePtr);
                m_fontTexturePtr = IntPtr.Zero;
            }

            m_isDisposed = true;
        }
    }
}