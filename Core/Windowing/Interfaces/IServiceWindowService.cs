namespace SDL2Engine.Core.Windowing.Interfaces
{
    internal interface IServiceWindowService 
    {
        IntPtr CreateWindowOpenGL();
        IntPtr CreateWindowSDL();
        void SetWindowIcon(IntPtr window, string iconPath);
    }
}
