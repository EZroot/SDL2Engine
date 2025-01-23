namespace SDL2Engine.Core.Windowing.Interfaces
{
    public interface IWindowService 
    {
        public nint WindowPtr { get; }
        nint CreateWindow();
        void SetWindowIcon(IntPtr window, string iconPath);
    }
}
