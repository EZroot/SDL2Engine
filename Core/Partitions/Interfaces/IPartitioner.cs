using System.Numerics;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.Partitions.Interfaces;

public interface IPartitioner
{
    void Add(GameObject obj);
    void Remove(GameObject obj);
    void Update(GameObject obj, Vector2 oldPosition);
    IEnumerable<GameObject> GetObjectsInCell(Vector2 position);
    void RenderDebug(nint renderer, ICameraService cameraService = null);
}