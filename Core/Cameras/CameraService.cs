using System.Collections.Generic;
using System.Numerics;
using SDL2;
using SDL2Engine.Core.Cameras.Interfaces;
using SDL2Engine.Core.Utils;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace SDL2Engine.Core.Cameras
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

        public int CreateCamera(int windowWidth, int windowHeight)
        {
            int cameraId = _nextId++;
            
            //  Default camera (SDL)
            ICamera camera = new Camera(OpenTK.Mathematics.Vector3.Zero);
            if (PlatformInfo.RendererType == RendererType.OpenGlRenderer && PlatformInfo.PipelineType == PipelineType.Pipe2D)
            {
                // Camera with some additional GL params (Viewport, Projection)
                camera = new CameraGL(OpenTK.Mathematics.Vector3.Zero, windowWidth,windowHeight);
            }
            
            // 3D Pipeline
            if (PlatformInfo.RendererType == RendererType.OpenGlRenderer &&
                PlatformInfo.PipelineType == PipelineType.Pipe3D)
            {
                var aspect = windowWidth / (float)windowHeight;
                camera = new CameraGL3D(
                    new Vector3(0, 0, -10),    
                    new Vector3(0, 0, 0),    
                    Vector3.UnitY,           
                    80f,                     
                    aspect,
                    0.1f,                    
                    2000f                   
                );
            }
            _idToCamera[cameraId] = camera;
            Debug.Log($"<color=green>OpenGL Camera Created:</color> ID={cameraId}, Position={camera.Position}, Zoom={camera.Zoom}");
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
