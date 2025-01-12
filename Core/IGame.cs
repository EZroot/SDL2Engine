using SDL2;

namespace SDL2Engine.Core;

public interface IGame
{
    void Initialize(IServiceProvider serviceProvider);
    void Update(float deltaTime);
    void Render();
    void Shutdown();
}