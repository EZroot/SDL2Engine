using System.Numerics;

namespace SDL2Engine.Core.Rendering.Interfaces;

public interface ICameraService
{
    int CreateCamera(Vector2 initialPosition, float initialZoom = 1.0f);
    ICamera GetCamera(int cameraId);
    bool RemoveCamera(int cameraId);
    bool SetActiveCamera(int cameraId);
    ICamera GetActiveCamera();
    void Cleanup();
}