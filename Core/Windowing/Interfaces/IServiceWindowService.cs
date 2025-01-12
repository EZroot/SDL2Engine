namespace SDL2Engine.Core.Windowing.Interfaces
{
    public interface IServiceWindowService 
    {
        IntPtr CreateWindowOpenGL();
        IntPtr CreateWindowSDL();
        void SetWindowIcon(IntPtr window, string iconPath);
    }
}
