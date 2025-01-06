namespace SDL2Engine.Core.Rendering.Interfaces
{
    internal interface IServiceRenderService
    {
        IntPtr CreateRenderer(IntPtr window);
        IntPtr CreateOpenGLContext(IntPtr window);
        void GLMakeCurrent(IntPtr window, IntPtr glContext);
    }
}
