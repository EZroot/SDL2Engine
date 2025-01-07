namespace SDL2Engine.Core.GuiRenderer.Interfaces
{
    public interface IServiceGuiWindowService
    {
        void BindVariable<T>(string key, T variable);
        void BeginWindow(string title);
        void EndWindow();
        void Draw(string key);
    }
}