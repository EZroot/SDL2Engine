namespace SDL2Engine.Core;

public interface IGame
{
    void Initialize();
    void Update(float deltaTime);
    void Render();
    void Shutdown();
}