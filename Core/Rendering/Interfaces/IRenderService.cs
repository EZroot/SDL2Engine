using OpenTK.Mathematics;
using SDL2;

namespace SDL2Engine.Core.Cameras.Interfaces
{
    public interface IRenderService
    {
        nint RenderPtr { get; }
        OpenGLHandle OpenGLHandleGui { get; }
        OpenGLHandle OpenGLHandle2D { get; }
        OpenGLHandle OpenGLHandle3D { get; }
        public void DrawLine(Vector2 start, Vector2 end, Color4 color);
        public void DrawRect(Vector2 topLeft, Vector2 bottomRight, Color4 color);
        public void RenderDebugPrimitives(Matrix4 projection);
        nint CreateRenderer(nint window, SDL.SDL_RendererFlags renderFlags =
            SDL.SDL_RendererFlags.SDL_RENDERER_ACCELERATED);
    }
}
