namespace SDL2Engine.Core.Windowing.Interfaces
{
    public interface IWindowService 
    {
        public nint WindowPtr { get; }
        IntPtr CreateWindowOpenGL();
        IntPtr CreateWindowSDL();
        void SetWindowIcon(IntPtr window, string iconPath);
    }
}
