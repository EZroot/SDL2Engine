namespace SDL2Engine.Core.Addressables.Interfaces
{
    public interface IServiceImageLoader
    {
        void Initialize();
        IntPtr LoadImage(string path);
        void CleanUp();
    }
}
