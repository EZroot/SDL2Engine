using SDL2;
using SDL2Engine.Core.Partitions.Interfaces;

namespace SDL2Engine.Core;

public interface IGame
{
    void Initialize(IServiceProvider serviceProvider);
    void Update(float deltaTime);
    void Render();
    void RenderGui();
    void Shutdown();
}