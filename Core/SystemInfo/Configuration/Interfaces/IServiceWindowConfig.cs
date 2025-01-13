using SDL2Engine.Core.CoreSystem.Configuration.Components;

namespace SDL2Engine.Core.CoreSystem.Configuration
{
    public interface IServiceWindowConfig
    {
        WindowSettings Settings { get; }
        void Save();
        void Save(WindowSettings newSettings);
    }
}
