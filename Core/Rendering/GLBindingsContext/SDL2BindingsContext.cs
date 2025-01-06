using OpenTK;

namespace SDL2Engine.Core.Rendering.GLBindingsContext
{
    public class SDL2BindingsContext : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return SDL2.SDL.SDL_GL_GetProcAddress(procName);
        }
    }
}