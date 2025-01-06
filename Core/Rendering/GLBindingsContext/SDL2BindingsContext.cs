using OpenTK;
using OpenTK.Graphics.OpenGL4;
using SDL2;
using System;

namespace SDL2Engine.Core.Rendering.GLBindingsContext
{
    public class SDL2BindingsContext : IBindingsContext
    {
        public IntPtr GetProcAddress(string procName)
        {
            return SDL.SDL_GL_GetProcAddress(procName);
        }

        //Load default driver if needed
        public void Load()
        {
            SDL.SDL_GL_LoadLibrary(null);  
        }
    }
}
