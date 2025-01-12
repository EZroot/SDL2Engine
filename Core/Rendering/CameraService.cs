using SDL2Engine.Core.Rendering.Interfaces;
using System.Numerics;
using SDL2Engine.Core.Utils;

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

            Debug.Log($"<color=green>Camera Created:</color> ID={cameraId}, Position={camera.Position}, Zoom={camera.Zoom}");
            return cameraId;
        }

        /// <inheritdoc />
        public ICamera GetCamera(int cameraId)
        {
            if (_idToCamera.TryGetValue(cameraId, out var camera))
            {
                return camera;
            }
            Debug.Log($"<color=orange>WARNING: Camera ID {cameraId} not found.</color>");
            return null;
        }

        /// <inheritdoc />
        public bool RemoveCamera(int cameraId)
        {
            if (_idToCamera.Remove(cameraId))
            {
                Debug.Log($"Camera Removed: ID={cameraId}");

                if (_activeCameraId == cameraId)
                {
                    _activeCameraId = null;
                    Debug.Log($"Active Camera ID {cameraId} was removed. No active camera set.");
                }

                return true;
            }

            Debug.Log($"<color=orange>WARNING: Attempted to remove non-existent camera ID: {cameraId}</color>");
            return false;
        }

        /// <inheritdoc />
        public bool SetActiveCamera(int cameraId)
        {
            if (_idToCamera.ContainsKey(cameraId))
            {
                _activeCameraId = cameraId;
                Debug.Log($"Active Camera Set: ID={cameraId}");
                return true;
            }

            Debug.Log($"<color=orange>WARNING: Cannot set active camera. Camera ID {cameraId} not found.</color>");
            return false;
        }

        /// <inheritdoc />
        public ICamera GetActiveCamera()
        {
            if (_activeCameraId.HasValue && _idToCamera.TryGetValue(_activeCameraId.Value, out var camera))
            {
                return camera;
            }

            Debug.Log("<color=orange>WARNING: No active camera set.</color>");
            return null;
        }

        /// <inheritdoc />
        public void Cleanup()
        {
            _idToCamera.Clear();
            _nameToId.Clear();
            _activeCameraId = null;
            Debug.Log("CameraService Cleanup Completed. All cameras have been removed.");
        }
    }
}
