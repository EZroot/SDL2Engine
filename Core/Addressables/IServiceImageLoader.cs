namespace SDL2Engine.Core.Addressables
{
    public interface IServiceImageLoader
    {
        void Initialize();
        IntPtr LoadImage(string path);
        void Shutdown();
    }
}
