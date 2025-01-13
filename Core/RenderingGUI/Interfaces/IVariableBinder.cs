namespace SDL2Engine.Core.GuiRenderer
{
    public interface IVariableBinder
    {
        void BindVariable<T>(string key, T variable);
        void Draw(string key);
    }
}