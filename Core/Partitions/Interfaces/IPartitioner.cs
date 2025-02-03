using OpenTK.Mathematics;
using SDL2Engine.Core.Rendering.Interfaces;

namespace SDL2Engine.Core.Partitions.Interfaces;

public interface IPartitioner
{
    void Add(GameObject obj);
    void Remove(GameObject obj);
    void Update(GameObject obj);
    IEnumerable<GameObject> GetObjectsInCell(Vector3 position);
    public IEnumerable<GameObject> GetNeighbors(Vector3 position, float radius);
    void RenderDebug(nint renderer, ICameraService cameraService = null);
}