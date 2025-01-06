namespace SDL2Engine.Core.Windowing.Interfaces
{
    internal interface IServiceWindowService
    {
        IntPtr CreateWindowOpenGL();
        IntPtr CreateWindowSDL();
    }
}
