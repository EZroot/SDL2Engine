using SDL2Engine.Core.Rendering.Interfaces;
using System.Numerics;

namespace SDL2Engine.Core.Rendering
{
    public class CameraService : IServiceCameraService
    {
        private readonly Dictionary<int, ICamera> _idToCamera;
        private readonly Dictionary<string, int> _nameToId; 
        private int _nextId;
        private int? _activeCameraId;

        public CameraService()
        {
            _idToCamera = new Dictionary<int, ICamera>();
            _nameToId = new Dictionary<string, int>();
            _nextId = 1;
            _activeCameraId = null;
        }

        /// <inheritdoc />
        public int CreateCamera(Vector2 initialPosition, float initialZoom = 1.0f)
        {
            int cameraId = _nextId++;
            var camera = new Camera(initialPosition, initialZoom);
            _idToCamera[cameraId] = camera;

            Console.WriteLine($"Camera Created: ID={cameraId}, Position={camera.Position}, Zoom={camera.Zoom}");
            return cameraId;
        }

        /// <inheritdoc />
        public ICamera GetCamera(int cameraId)
        {
            if (_idToCamera.TryGetValue(cameraId, out var camera))
            {
                return camera;
            }
            Console.WriteLine($"<color=orange>WARNING: Camera ID {cameraId} not found.</color>");
            return null;
        }

        /// <inheritdoc />
        public bool RemoveCamera(int cameraId)
        {
            if (_idToCamera.Remove(cameraId))
            {
                Console.WriteLine($"Camera Removed: ID={cameraId}");

                if (_activeCameraId == cameraId)
                {
                    _activeCameraId = null;
                    Console.WriteLine($"Active Camera ID {cameraId} was removed. No active camera set.");
                }

                return true;
            }

            Console.WriteLine($"<color=orange>WARNING: Attempted to remove non-existent camera ID: {cameraId}</color>");
            return false;
        }

        /// <inheritdoc />
        public bool SetActiveCamera(int cameraId)
        {
            if (_idToCamera.ContainsKey(cameraId))
            {
                _activeCameraId = cameraId;
                Console.WriteLine($"Active Camera Set: ID={cameraId}");
                return true;
            }

            Console.WriteLine($"<color=orange>WARNING: Cannot set active camera. Camera ID {cameraId} not found.</color>");
            return false;
        }

        /// <inheritdoc />
        public ICamera GetActiveCamera()
        {
            if (_activeCameraId.HasValue && _idToCamera.TryGetValue(_activeCameraId.Value, out var camera))
            {
                return camera;
            }

            Console.WriteLine("<color=orange>WARNING: No active camera set.</color>");
            return null;
        }

        /// <inheritdoc />
        public void Cleanup()
        {
            _idToCamera.Clear();
            _nameToId.Clear();
            _activeCameraId = null;
            Console.WriteLine("CameraService Cleanup Completed. All cameras have been removed.");
        }
    }
}
