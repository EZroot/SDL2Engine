using System.Numerics;

namespace SDL2Engine.Core.Rendering.Interfaces;

public interface ICameraService
{
    int CreateCamera(Vector2 initialPosition, float initialZoom = 1.0f);

    int CreateOpenGLCamera(Vector2 initialPosition, float viewportWidth, float viewportHeight,
        float initialZoom = 1.0f);
    ICamera GetCamera(int cameraId);
    bool RemoveCamera(int cameraId);
    bool SetActiveCamera(int cameraId);
    ICamera GetActiveCamera();
    void Cleanup();
}