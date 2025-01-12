namespace SDL2Engine.Core.Windowing.Interfaces
{
    public interface IServiceWindowService 
    {
        public nint Window { get; }
        IntPtr CreateWindowOpenGL();
        IntPtr CreateWindowSDL();
        void SetWindowIcon(IntPtr window, string iconPath);
    }
}
