using SDL2Engine.Core.Rendering.Interfaces;
using System.Collections.Generic;
using System.Numerics;
using SDL2Engine.Core.Utils;

namespace SDL2Engine.Core.Rendering
{
    public class CameraService : ICameraService
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

        /// <summary>
        /// Creates a standard 2D camera.
        /// </summary>
        public int CreateCamera(Vector2 initialPosition, float initialZoom = 1.0f)
        {
            int cameraId = _nextId++;
            var camera = new Camera(initialPosition, initialZoom);
            _idToCamera[cameraId] = camera;

            Debug.Log($"<color=green>Camera Created:</color> ID={cameraId}, Position={camera.Position}, Zoom={camera.Zoom}");
            return cameraId;
        }

        /// <summary>
        /// Creates an OpenGL-specific camera with a projection matrix.
        /// </summary>
        public int CreateOpenGLCamera(Vector2 initialPosition, float viewportWidth, float viewportHeight, float initialZoom = 1.0f)
        {
            int cameraId = _nextId++;
            var camera = new CameraGL(initialPosition, viewportWidth, viewportHeight, initialZoom);
            _idToCamera[cameraId] = camera;

            Debug.Log($"<color=green>OpenGL Camera Created:</color> ID={cameraId}, Position={camera.Position}, Zoom={camera.Zoom}, Viewport={viewportWidth}x{viewportHeight}");
            return cameraId;
        }

        /// <summary>
        /// Retrieves a camera by ID.
        /// </summary>
        public ICamera GetCamera(int cameraId)
        {
            if (_idToCamera.TryGetValue(cameraId, out var camera))
            {
                return camera;
            }
            Debug.Log($"<color=orange>WARNING: Camera ID {cameraId} not found.</color>");
            return null;
        }

        /// <summary>
        /// Removes a camera by ID.
        /// </summary>
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

            Debug.LogWarning($"Attempted to remove non-existent camera ID: {cameraId}</color>");
            return false;
        }

        /// <summary>
        /// Sets the active camera by ID.
        /// </summary>
        public bool SetActiveCamera(int cameraId)
        {
            if (_idToCamera.ContainsKey(cameraId))
            {
                _activeCameraId = cameraId;
                Debug.Log($"Active Camera Set: ID={cameraId}");
                return true;
            }

            Debug.LogWarning($"Cannot set active camera. Camera ID {cameraId} not found.</color>");
            return false;
        }

        /// <summary>
        /// Retrieves the active camera.
        /// </summary>
        public ICamera GetActiveCamera()
        {
            if (_activeCameraId.HasValue && _idToCamera.TryGetValue(_activeCameraId.Value, out var camera))
            {
                return camera;
            }

            Debug.Log("<color=orange>WARNING: No active camera set.</color>");
            return null;
        }

        /// <summary>
        /// Cleans up all cameras.
        /// </summary>
        public void Cleanup()
        {
            _idToCamera.Clear();
            _nameToId.Clear();
            _activeCameraId = null;
            Debug.Log("CameraService Cleanup Completed. All cameras have been removed.");
        }
    }
}
