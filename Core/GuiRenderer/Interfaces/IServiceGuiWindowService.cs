namespace SDL2Engine.Core.GuiRenderer.Interfaces
{
    public interface IServiceGuiWindowService
    {
        void BeginWindow(string title);
        void EndWindow();
        void Draw(string key);
    }
}