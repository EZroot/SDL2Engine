using SDL2Engine.Core.Configuration.Components;

namespace SDL2Engine.Core.Configuration
{
    internal interface IServiceWindowConfig
    {
        WindowSettings Settings { get; }
        void Save();
    }
}
