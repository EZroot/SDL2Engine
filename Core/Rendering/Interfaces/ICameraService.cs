using System.Numerics;

namespace SDL2Engine.Core.Rendering.Interfaces;

public interface ICameraService
{
    int CreateCamera(int windowWidth, int windowHeight);
    ICamera GetCamera(int cameraId);
    bool RemoveCamera(int cameraId);
    bool SetActiveCamera(int cameraId);
    ICamera GetActiveCamera();
    void Cleanup();
}